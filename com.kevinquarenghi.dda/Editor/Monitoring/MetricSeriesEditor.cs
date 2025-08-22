using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using KevinQuarenghi.DDA.Monitoring;

namespace KevinQuarenghi.DDA.Editor.Monitoring
{
    /// <summary>
    /// Custom inspector per <see cref="MetricSeries"/>, che:
    /// 1) disegna la serie temporale su finestra mobile (ultimi maxSamples campioni),
    /// 2) traccia la lowerBound (rosso) e upperBound (verde) sempre visibili,
    /// 3) infine disegna i controlli per modificare i parametri con vincoli [0–1] e lower<=upper.
    /// </summary>
    [CustomEditor(typeof(MetricSeries))]
    public class MetricSeriesEditor : UnityEditor.Editor
    {
        private const int k_Padding = 8;
        private const float k_GraphLineWidth = 2f;
        private const int k_NumTicksX = 5;
        private const int k_NumTicksY = 5;

        public override void OnInspectorGUI()
        {
            // Aggiorna SerializedObject
            serializedObject.Update();

            // Disegna il grafico
            DrawSeriesChart();
            EditorGUILayout.Space();

            // Propietà per il SO
            SerializedProperty propMetricName = serializedObject.FindProperty("metricName");
            SerializedProperty propLower = serializedObject.FindProperty("lowerBound");
            SerializedProperty propUpper = serializedObject.FindProperty("upperBound");
            SerializedProperty propMax = serializedObject.FindProperty("maxSamples");
            SerializedProperty propTimestamps = serializedObject.FindProperty("timestamps");
            SerializedProperty propValues = serializedObject.FindProperty("values");

            // Campo nome metrica
            EditorGUILayout.PropertyField(propMetricName);

            // Slider con vincoli: 0 <= lower <= upper <= 1
            float lower = propLower.floatValue;
            float upper = propUpper.floatValue;
            lower = EditorGUILayout.Slider("Lower Bound", lower, 0f, 1f);
            upper = EditorGUILayout.Slider("Upper Bound", upper, 0f, 1f);
            // Applica vincoli
            lower = Mathf.Clamp(lower, 0f, upper);
            upper = Mathf.Clamp(upper, lower, 1f);
            propLower.floatValue = lower;
            propUpper.floatValue = upper;

            // Resto delle proprietà
            EditorGUILayout.PropertyField(propMax);
            EditorGUILayout.PropertyField(propTimestamps, true);
            EditorGUILayout.PropertyField(propValues, true);

            // Applica modifiche
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSeriesChart()
        {
            var s = (MetricSeries)target;
            int count = s.Count;
            if (count < 2)
            {
                EditorGUILayout.HelpBox(
                    "Aggiungi almeno due campioni in Play Mode per vedere il grafico.",
                    MessageType.Info);
                return;
            }

            int start = Mathf.Max(0, count - s.maxSamples);
            float t0 = s.timestamps[start];
            float t1 = s.timestamps[count - 1];
            float duration = t1 - t0;
            if (duration <= 0f) duration = 1f;

            float vmin = float.MaxValue, vmax = float.MinValue;
            for (int i = start; i < count; i++)
            {
                float v = s.values[i];
                vmin = Mathf.Min(vmin, v);
                vmax = Mathf.Max(vmax, v);
            }
            vmin = Mathf.Min(vmin, s.lowerBound);
            vmax = Mathf.Max(vmax, s.upperBound);
            if (Mathf.Approximately(vmin, vmax))
            {
                vmin -= 0.1f; vmax += 0.1f;
            }

            Rect rect = GUILayoutUtility.GetRect(350, 180);
            rect = EditorGUI.IndentedRect(rect);
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f));
            float w = rect.width - 2 * k_Padding;
            float h = rect.height - 2 * k_Padding;

            // Reticolo
            Handles.BeginGUI();
            Handles.color = new Color(1, 1, 1, 0.1f);
            for (int i = 0; i <= k_NumTicksX; i++)
            {
                float x = rect.x + k_Padding + w * i / k_NumTicksX;
                Handles.DrawLine(
                    new Vector3(x, rect.y + k_Padding, 0),
                    new Vector3(x, rect.yMax - k_Padding, 0));
            }
            for (int j = 0; j <= k_NumTicksY; j++)
            {
                float y = rect.yMax - k_Padding - h * j / k_NumTicksY;
                Handles.DrawLine(
                    new Vector3(rect.x + k_Padding, y, 0),
                    new Vector3(rect.xMax - k_Padding, y, 0));
            }
            Handles.EndGUI();

            // Assi
            GUI.color = Color.white;
            var style = EditorStyles.miniLabel;
            for (int i = 0; i <= k_NumTicksX; i++)
            {
                float t = Mathf.Lerp(t0, t1, i / (float)k_NumTicksX);
                float x = rect.x + k_Padding + w * i / k_NumTicksX;
                GUI.Label(new Rect(x - 15, rect.yMax - 14, 40, 12), t.ToString("0.##"), style);
            }
            for (int j = 0; j <= k_NumTicksY; j++)
            {
                float v = Mathf.Lerp(vmin, vmax, j / (float)k_NumTicksY);
                float y = rect.yMax - k_Padding - h * j / k_NumTicksY;
                GUI.Label(new Rect(rect.x + 2, y - 7, 40, 12), v.ToString("0.##"), style);
            }

            // Bound orizzontali
            float yLow = rect.yMax - k_Padding - (s.lowerBound - vmin) / (vmax - vmin) * h;
            float yHigh = rect.yMax - k_Padding - (s.upperBound - vmin) / (vmax - vmin) * h;
            Handles.BeginGUI();
            Handles.color = Color.red;
            Handles.DrawLine(
                new Vector3(rect.x + k_Padding, yLow, 0),
                new Vector3(rect.xMax - k_Padding, yLow, 0));
            Handles.color = Color.green;
            Handles.DrawLine(
                new Vector3(rect.x + k_Padding, yHigh, 0),
                new Vector3(rect.xMax - k_Padding, yHigh, 0));
            Handles.EndGUI();

            // Curva dati
            var pts = new Vector3[count - start];
            for (int i = start; i < count; i++)
            {
                float nx = (s.timestamps[i] - t0) / duration;
                float ny = (s.values[i] - vmin) / (vmax - vmin);
                float x = rect.x + k_Padding + nx * w;
                float y = rect.yMax - k_Padding - ny * h;
                pts[i - start] = new Vector3(x, y, 0);
            }
            Handles.BeginGUI();
            Handles.color = Color.cyan;
            Handles.DrawAAPolyLine(k_GraphLineWidth, pts);
            Handles.EndGUI();
        }
    }
}