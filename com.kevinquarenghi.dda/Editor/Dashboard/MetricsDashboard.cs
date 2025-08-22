using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using KevinQuarenghi.DDA.Monitoring;

namespace KevinQuarenghi.DDA.Editor.Dashboard
{
    /// <summary>
    /// EditorWindow che mostra una “small multiples” dashboard:
    /// un grafico separato per ciascuna MetricSeries selezionata,
    /// con asse temporale normalizzato (0→maxDuration),
    /// griglia leggera, titolo e margini dedicati.
    /// </summary>
    public class MetricsDashboard : EditorWindow
    {
        private const int k_MarginLeft = 50;   // spazio per Y‐label
        private const int k_MarginRight = 20;   // spazio a destra del grafico
        private const int k_MarginTop = 4;    // spazio fra titolo e grafico
        private const int k_MarginBottom = 30;   // spazio sotto il grafico per X‐label
        private const int k_TitleHeight = 18;   // altezza riservata al titolo
        private const float k_LineWidth = 2f;   // spessore delle curve
        private const int k_ChartRowHeight = 120;  // altezza di ogni mini‐chart
        private const int k_NumXTicks = 4;    // tick asse X per small multiples
        private const int k_NumYTicks = 3;    // tick asse Y per small multiples

        private List<MetricSeries> _allSeries;
        private Dictionary<MetricSeries, bool> _selection;
        private Vector2 _scrollPos;

        [MenuItem("Window/DDA/Metrics Dashboard")]
        public static void OpenWindow()
        {
            GetWindow<MetricsDashboard>("DDA Metrics Dashboard");
        }

        private void OnEnable()
        {
            // carica tutte le MetricSeries e azzera la selezione
            _allSeries = FindObjectsOfType<MetricSeries>()
                .Concat(AssetDatabase.FindAssets("t:MetricSeries")
                    .Select(guid => AssetDatabase.LoadAssetAtPath<MetricSeries>(
                        AssetDatabase.GUIDToAssetPath(guid))))
                .Where(s => s != null)
                .Distinct()
                .ToList();

            _selection = _allSeries.ToDictionary(s => s, s => false);
        }

        private void OnGUI()
        {
            DrawToolbar();
            DrawSelection();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            DrawSmallMultiples();
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Export CSV", EditorStyles.toolbarButton))
                    ExportCSV();
                GUILayout.FlexibleSpace();
            }
        }

        private void DrawSelection()
        {
            EditorGUILayout.LabelField("Select Series:", EditorStyles.boldLabel);
            foreach (var s in _allSeries)
            {
                _selection[s] = EditorGUILayout.ToggleLeft(s.metricName, _selection[s]);
            }
            EditorGUILayout.Space(8);
        }

        private void DrawSmallMultiples()
        {
            var selected = _selection.Where(kv => kv.Value)
                                     .Select(kv => kv.Key)
                                     .ToList();
            if (!selected.Any())
            {
                EditorGUILayout.HelpBox("Select at least one series to display the charts.", MessageType.Info);
                return;
            }

            // calcola la durata massima
            float maxDuration = selected
                .Select(s => s.timestamps.Last() - s.timestamps.First())
                .Max();

            // calcola il range Y globale
            float vMin = selected.Min(s => s.values.Min());
            float vMax = selected.Max(s => s.values.Max());
            if (Mathf.Approximately(vMin, vMax))
            {
                vMin -= 0.1f; vMax += 0.1f;
            }

            foreach (var s in selected)
            {
                // rettangolo di altezza fissa
                Rect row = EditorGUILayout.GetControlRect(
                    false,
                    k_ChartRowHeight,
                    GUILayout.ExpandWidth(true)
                );

                // 1) Titolo sopra il grafico
                Rect titleRect = new Rect(
                    row.x + k_MarginLeft,
                    row.y,
                    row.width - k_MarginLeft - k_MarginRight,
                    k_TitleHeight
                );
                EditorGUI.LabelField(titleRect, s.metricName, EditorStyles.boldLabel);

                // 2) Rect del grafico, spostato in basso di k_TitleHeight
                Rect chart = new Rect(
                    row.x + k_MarginLeft,
                    row.y + k_TitleHeight + k_MarginTop,
                    row.width - k_MarginLeft - k_MarginRight,
                    row.height - k_TitleHeight - k_MarginTop - k_MarginBottom
                );

                // sfondo
                EditorGUI.DrawRect(chart, new Color(0.12f, 0.12f, 0.12f));
                // griglia e assi
                DrawGridAndAxes(chart, maxDuration, vMin, vMax, k_NumXTicks, k_NumYTicks);
                // curva dati
                DrawSeries(s, chart, maxDuration, vMin, vMax);

                EditorGUILayout.Space(4);
            }
        }

        private void DrawGridAndAxes(
            Rect r,
            float maxDuration,
            float vMin,
            float vMax,
            int xTicks,
            int yTicks)
        {
            Handles.BeginGUI();
            Handles.color = new Color(1, 1, 1, 0.1f);
            // vertical grid
            for (int i = 0; i <= xTicks; i++)
            {
                float x = r.x + i * (r.width / xTicks);
                Handles.DrawLine(new Vector3(x, r.y, 0), new Vector3(x, r.yMax, 0));
            }
            // horizontal grid
            for (int j = 0; j <= yTicks; j++)
            {
                float y = r.y + j * (r.height / yTicks);
                Handles.DrawLine(new Vector3(r.x, y, 0), new Vector3(r.xMax, y, 0));
            }
            // assi bianchi
            Handles.color = Color.white;
            Handles.DrawLine(new Vector3(r.x, r.yMax, 0), new Vector3(r.xMax, r.yMax, 0)); // x
            Handles.DrawLine(new Vector3(r.x, r.y, 0), new Vector3(r.x, r.yMax, 0)); // y
            Handles.EndGUI();

            // label X
            for (int i = 0; i <= xTicks; i++)
            {
                float t = maxDuration * i / xTicks;
                float x = r.x + i * (r.width / xTicks) - 10f;
                GUI.Label(new Rect(x, r.yMax + 2, 40, 12), t.ToString("0.##"), EditorStyles.miniLabel);
            }
            // label Y
            for (int j = 0; j <= yTicks; j++)
            {
                float v = vMax - (vMax - vMin) * j / yTicks;
                float y = r.y + j * (r.height / yTicks) - 6f;
                GUI.Label(new Rect(r.x - k_MarginLeft + 5, y, 40, 12), v.ToString("0.##"), EditorStyles.miniLabel);
            }
        }

        private void DrawSeries(
            MetricSeries s,
            Rect r,
            float maxDuration,
            float vMin,
            float vMax)
        {
            int cnt = s.Count;
            if (cnt < 2) return;

            Vector3[] pts = new Vector3[cnt];
            float t0 = s.timestamps.First();
            for (int i = 0; i < cnt; i++)
            {
                float relT = (s.timestamps[i] - t0) / Mathf.Max(1e-6f, maxDuration);
                float relV = (s.values[i] - vMin) / (vMax - vMin);
                pts[i] = new Vector3(
                    r.x + relT * r.width,
                    r.yMax - relV * r.height,
                    0
                );
            }

            Handles.BeginGUI();
            Handles.color = GetColor(s);
            Handles.DrawAAPolyLine(k_LineWidth, pts);
            Handles.EndGUI();
        }

        private void ExportCSV()
        {
            var sel = _selection.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
            if (!sel.Any()) return;

            float maxDuration = sel.Select(s => s.timestamps.Last() - s.timestamps.First()).Max();
            int steps = sel.Max(s => s.Count);

            var sb = new System.Text.StringBuilder();
            sb.Append("Time;").AppendLine(string.Join(";", sel.Select(s => s.metricName)));

            for (int i = 0; i < steps; i++)
            {
                float t = maxDuration * i / Mathf.Max(1, steps - 1);
                sb.Append(t.ToString("F3"));
                foreach (var s in sel)
                {
                    float v = (i < s.Count) ? s.values[i] : 0f;
                    sb.Append(";" + v.ToString("F3"));
                }
                sb.AppendLine();
            }

            string path = EditorUtility.SaveFilePanel("Save Metrics CSV", "", "metrics.csv", "csv");
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path, sb.ToString());
                AssetDatabase.Refresh();
                Debug.Log($"Exported CSV at {path}");
            }
        }

        private Color GetColor(MetricSeries s)
        {
            int idx = _allSeries.IndexOf(s);
            float t = idx / (float)Mathf.Max(1, _allSeries.Count - 1);
            return Color.Lerp(Color.cyan, Color.magenta, t);
        }
    }
}
