using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using KevinQuarenghi.DDA.Config;
using KevinQuarenghi.DDA.Engine;

namespace KevinQuarenghi.DDA.Editor.Config
{
    [CustomEditor(typeof(FuzzyConfigSO))]
    public class FuzzyConfigSOEditor : UnityEditor.Editor
    {
        private ReorderableList _varsList, _rulesList;
        private const float k_RemoveButtonWidth = 18f;
        private const float k_RemoveButtonPadding = 4f;
        private const float k_RowPadding = 2f;

        private readonly Dictionary<int, float> _cacheVarHeight = new Dictionary<int, float>();
        private readonly Dictionary<int, float> _cacheRuleHeight = new Dictionary<int, float>();
        private readonly Dictionary<string, Texture2D> _graphCache = new Dictionary<string, Texture2D>();
        private readonly Dictionary<string, float> _termTestValues = new Dictionary<string, float>();

        private void OnEnable()
        {
            // VARIABLES
            _varsList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("variables"),
                true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Variables"),
                elementHeightCallback = GetVariableHeight,
                drawElementCallback = DrawVariableElement
            };
            _varsList.onChangedCallback = _ => _cacheVarHeight.Clear();

            // RULES (unchanged)
            _rulesList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("rules"),
                true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Rules"),
                elementHeightCallback = GetRuleHeight,
                drawElementCallback = DrawRuleElement
            };
            _rulesList.onChangedCallback = _ => _cacheRuleHeight.Clear();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _varsList.DoLayoutList();
            GUILayout.Space(10);
            _rulesList.DoLayoutList();
            GUILayout.Space(10);
            if (GUILayout.Button("Export to JSON"))
                FuzzyConfigUtility.Export();
            serializedObject.ApplyModifiedProperties();
        }

        // ————————————————
        //     ALTEZZA ELEMENTI
        // ————————————————

        private float GetVariableHeight(int index)
        {
            if (_cacheVarHeight.TryGetValue(index, out var h)) return h;
            var varP = _varsList.serializedProperty.GetArrayElementAtIndex(index);
            var termsP = varP.FindPropertyRelative("terms");
            int count = termsP.arraySize;

            float perTerm = EditorGUIUtility.singleLineHeight
                          + k_RowPadding
                          + 40f
                          + EditorGUIUtility.singleLineHeight
                          + k_RowPadding;

            h = EditorGUIUtility.singleLineHeight
              + k_RowPadding
              + EditorGUIUtility.singleLineHeight
              + k_RowPadding
              + count * perTerm
              + k_RowPadding;

            _cacheVarHeight[index] = h;
            return h;
        }

        private float GetRuleHeight(int index)
        {
            if (_cacheRuleHeight.TryGetValue(index, out var h)) return h;
            var ruleP = _rulesList.serializedProperty.GetArrayElementAtIndex(index);
            int c = ruleP.FindPropertyRelative("conditions").arraySize;
            int a = ruleP.FindPropertyRelative("actions").arraySize;

            h = k_RowPadding
              + (c + 1) * (EditorGUIUtility.singleLineHeight + k_RowPadding)
              + k_RowPadding
              + (a + 1) * (EditorGUIUtility.singleLineHeight + k_RowPadding)
              + EditorGUIUtility.singleLineHeight
              + k_RowPadding;

            _cacheRuleHeight[index] = h;
            return h;
        }

        // ————————————————
        //    DRAW VARIABLES
        // ————————————————

        private void DrawVariableElement(Rect rect, int index, bool active, bool focus)
        {
            var varP = _varsList.serializedProperty.GetArrayElementAtIndex(index);
            var nameP = varP.FindPropertyRelative("name");
            var termsP = varP.FindPropertyRelative("terms");

            rect.y += k_RowPadding;
            // 1) Variable Name
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                nameP, new GUIContent("Variable Name"));

            // 2) Terms header + [+]
            float y0 = rect.y + EditorGUIUtility.singleLineHeight + k_RowPadding;
            EditorGUI.LabelField(
                new Rect(rect.x, y0, 60, EditorGUIUtility.singleLineHeight),
                "Terms:");
            if (GUI.Button(
                new Rect(rect.x + rect.width - k_RemoveButtonWidth - k_RemoveButtonPadding,
                         y0,
                         k_RemoveButtonWidth,
                         k_RemoveButtonWidth),
                "+"))
            {
                termsP.arraySize++;
                _cacheVarHeight.Clear();
                serializedObject.ApplyModifiedProperties();
                Repaint();
                return;
            }

            // 3) Draw each term
            DrawTermsList(
                termsP,
                rect.x + 16,
                y0 + EditorGUIUtility.singleLineHeight + k_RowPadding,
                rect.width - 16,
                nameP.stringValue);
        }

        private void DrawTermsList(
            SerializedProperty termsP,
            float x, float y, float width,
            string varName)
        {
            float rowH = EditorGUIUtility.singleLineHeight + k_RowPadding + 40f + EditorGUIUtility.singleLineHeight + k_RowPadding;

            for (int t = 0; t < termsP.arraySize; t++)
            {
                var termP = termsP.GetArrayElementAtIndex(t);
                var lblP = termP.FindPropertyRelative("label");
                var ptsP = termP.FindPropertyRelative("points");

                float yy = y + t * rowH;

                // — label —
                Rect lblRect = new Rect(x, yy, 100, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(lblRect, lblP, GUIContent.none);

                // — duplicate —
                if (GUI.Button(
                    new Rect(lblRect.xMax + 2, yy, k_RemoveButtonWidth, k_RemoveButtonWidth),
                    "+"))
                {
                    termsP.InsertArrayElementAtIndex(t);
                    _cacheVarHeight.Clear();
                    serializedObject.ApplyModifiedProperties();
                    Repaint();
                    return;
                }

                // — shape popup —
                Rect shapeRect = new Rect(lblRect.xMax + 2 + k_RemoveButtonWidth + 2,
                                          yy,
                                          80,
                                          EditorGUIUtility.singleLineHeight);
                string[] opts = { "Triangle", "Trapezoid" };
                int shapeIdx = ptsP.arraySize == 4 ? 1 : 0;
                int sel = EditorGUI.Popup(shapeRect, shapeIdx, opts);
                int len = sel == 1 ? 4 : 3;
                if (ptsP.arraySize != len) ptsP.arraySize = len;

                // — point fields —
                float usedW = 100f + (k_RemoveButtonWidth + 2) + 80f + (k_RemoveButtonWidth + 2);
                float ptsW = (width - usedW - k_RemoveButtonWidth) / len;
                float px0 = shapeRect.xMax + 2;
                for (int i = 0; i < len; i++)
                {
                    var el = ptsP.GetArrayElementAtIndex(i);
                    Rect pRect = new Rect(px0 + i * (ptsW + 2),
                                          yy,
                                          ptsW,
                                          EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(pRect, el, GUIContent.none);
                }

                // — remove —
                Rect remR = new Rect(x + width - k_RemoveButtonWidth - k_RemoveButtonPadding,
                                     yy,
                                     k_RemoveButtonWidth,
                                     k_RemoveButtonWidth);
                if (GUI.Button(remR, "-"))
                {
                    termsP.DeleteArrayElementAtIndex(t);
                    _cacheVarHeight.Clear();
                    serializedObject.ApplyModifiedProperties();
                    Repaint();
                    return;
                }

                // — mini-graph —
                Rect graphR = new Rect(x,
                                       yy + EditorGUIUtility.singleLineHeight + k_RowPadding,
                                       width,
                                       40);
                string key = varName + ":" + lblP.stringValue;
                if (!_graphCache.TryGetValue(key, out var tex))
                {
                    tex = BuildGraphTexture((int)graphR.width, (int)graphR.height, ptsP);
                    _graphCache[key] = tex;
                }
                if (tex != null)
                    GUI.DrawTexture(graphR, tex, ScaleMode.StretchToFill);

                // — slider + μ —
                Rect sliderR = new Rect(x,
                                        graphR.yMax + 2,
                                        width - 50,
                                        EditorGUIUtility.singleLineHeight);
                if (!_termTestValues.ContainsKey(key))
                    _termTestValues[key] = 0.5f;
                _termTestValues[key] = EditorGUI.Slider(
                    sliderR,
                    _termTestValues[key],
                    0f,
                    1f);

                float mu = CalculateMembership(_termTestValues[key], ptsP);
                Rect muR = new Rect(sliderR.xMax + 4,
                                    sliderR.y,
                                    46,
                                    EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(muR, mu.ToString("F2"));
            }
        }

        // ————————————————
        //     DRAW RULES
        // ————————————————

        private void DrawRuleElement(Rect rect, int index, bool active, bool focus)
        {
            var ruleP = _rulesList.serializedProperty.GetArrayElementAtIndex(index);
            var conds = ruleP.FindPropertyRelative("conditions");
            var acts = ruleP.FindPropertyRelative("actions");
            rect.y += k_RowPadding;

            // Conditions
            float y1 = rect.y + EditorGUIUtility.singleLineHeight + k_RowPadding;
            EditorGUI.LabelField(
                new Rect(rect.x, y1, rect.width - 20, EditorGUIUtility.singleLineHeight),
                "Conditions:");
            if (GUI.Button(
                new Rect(rect.x + rect.width - 20, y1, 18, 18), "+"))
                conds.arraySize++;
            DrawConditionList(conds,
                              rect.x + 16,
                              y1 + EditorGUIUtility.singleLineHeight + k_RowPadding,
                              rect.width - 16);

            // Actions
            float y2 = y1 + (conds.arraySize + 1) * (EditorGUIUtility.singleLineHeight + k_RowPadding) + k_RowPadding;
            EditorGUI.LabelField(
                new Rect(rect.x, y2, rect.width - 20, EditorGUIUtility.singleLineHeight),
                "Actions:");
            if (GUI.Button(
                new Rect(rect.x + rect.width - 20, y2, 18, 18), "+"))
                acts.arraySize++;
            DrawActionList(acts,
                           rect.x + 16,
                           y2 + EditorGUIUtility.singleLineHeight + k_RowPadding,
                           rect.width - 16);
        }

        private void DrawConditionList(SerializedProperty listProp, float x, float y, float width)
        {
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var elem = listProp.GetArrayElementAtIndex(i);
                var varProp = elem.FindPropertyRelative("variable");
                var termProp = elem.FindPropertyRelative("term");
                var orProp = elem.FindPropertyRelative("useOrWithPrev");
                float yy = y + i * (EditorGUIUtility.singleLineHeight + k_RowPadding);

                EditorGUI.PropertyField(
                    new Rect(x, yy, width * 0.5f - 60, EditorGUIUtility.singleLineHeight),
                    varProp, GUIContent.none);
                EditorGUI.PropertyField(
                    new Rect(x + width * 0.5f - 56, yy, width * 0.5f - 60, EditorGUIUtility.singleLineHeight),
                    termProp, GUIContent.none);

                Rect remR = new Rect(x + width - 20, yy, 18, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(remR, "-"))
                {
                    listProp.DeleteArrayElementAtIndex(i);
                    continue;
                }

                if (i > 0)
                {
                    Rect popR = new Rect(x + width - 90, yy, 60, EditorGUIUtility.singleLineHeight);
                    int choice = orProp.boolValue ? 1 : 0;
                    choice = EditorGUI.Popup(popR, choice, new[] { "AND", "OR" });
                    orProp.boolValue = (choice == 1);
                }
            }
        }

        private void DrawActionList(SerializedProperty listProp, float x, float y, float width)
        {
            for (int i = 0; i < listProp.arraySize; i++)
            {
                var elem = listProp.GetArrayElementAtIndex(i);
                var varProp = elem.FindPropertyRelative("variable");
                var termProp = elem.FindPropertyRelative("term");
                float yy = y + i * (EditorGUIUtility.singleLineHeight + k_RowPadding);

                EditorGUI.PropertyField(
                    new Rect(x, yy, width * 0.5f - 60, EditorGUIUtility.singleLineHeight),
                    varProp, GUIContent.none);
                EditorGUI.PropertyField(
                    new Rect(x + width * 0.5f - 56, yy, width * 0.5f - 60, EditorGUIUtility.singleLineHeight),
                    termProp, GUIContent.none);
                if (GUI.Button(new Rect(x + width - 20, yy, 18, 18), "-"))
                    listProp.DeleteArrayElementAtIndex(i);
            }
        }

        // ————————————————
        // GRAFICO & MEMBERSHIP
        // ————————————————

        private Texture2D BuildGraphTexture(int w, int h, SerializedProperty ptsP)
        {
            if (w <= 0 || h <= 0) return null;
            var tex = new Texture2D(w, h)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            Color clear = new Color(0, 0, 0, 0);
            for (int yy = 0; yy < h; yy++)
                for (int xx = 0; xx < w; xx++)
                    tex.SetPixel(xx, yy, clear);

            for (int i = 0; i < w; i++)
            {
                float t = (float)i / (w - 1);
                float mu = CalculateMembership(t, ptsP);
                int py = Mathf.Clamp(Mathf.RoundToInt(mu * (h - 1)), 0, h - 1);
                tex.SetPixel(i, py, Color.white);
            }
            tex.Apply();
            return tex;
        }

        private float CalculateMembership(float x, SerializedProperty ptsProp)
        {
            int len = ptsProp.arraySize;
            var p = new float[len];
            for (int i = 0; i < len; i++)
                p[i] = ptsProp.GetArrayElementAtIndex(i).floatValue;

            if (len == 3)
            {
                float a = p[0], b = p[1], c = p[2];
                if (Mathf.Approximately(a, b))
                    return x <= b ? 1f : (x >= c ? 0f : (c - x) / (c - b));
                if (Mathf.Approximately(b, c))
                    return x >= b ? 1f : (x <= a ? 0f : (x - a) / (b - a));
                if (x <= a || x >= c) return 0f;
                return x < b ? (x - a) / (b - a) : (c - x) / (c - b);
            }
            if (len == 4)
            {
                float a = p[0], b = p[1], c = p[2], d = p[3];
                if (Mathf.Approximately(a, b))
                    return x <= b ? 1f : (x >= d ? 0f : (x <= c ? 1f : (d - x) / (d - c)));
                if (Mathf.Approximately(c, d))
                    return x >= c ? 1f : (x <= a ? 0f : (x >= b ? 1f : (x - a) / (b - a)));
                if (x <= a || x >= d) return 0f;
                if (x >= b && x <= c) return 1f;
                return x < b ? (x - a) / (b - a) : (d - x) / (d - c);
            }
            return 0f;
        }
    }
}
