#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using KevinQuarenghi.DDA.Core;

namespace KevinQuarenghi.DDA.Monitoring
{
    /// <summary>
    /// Editor-only component that logs, at each DDA evaluation step,
    /// the normalized metrics and the corresponding deltas
    /// to a CSV file for offline ML training.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class MLTrainingDataLogger : MonoBehaviour
    {
        /// <summary>
        /// Reference to the scene’s <see cref="DDAController"/>,
        /// whose <see cref="DDAController.OnPostEvaluation"/> event
        /// provides the timestamp, normalized metrics, and deltas.
        /// </summary>
        [Tooltip("Il DDAController in scena")]
        public DDAController controller;

        /// <summary>
        /// File name (under Application.persistentDataPath) where
        /// the CSV will be written. Existing file is overwritten on Awake.
        /// </summary>
        [Tooltip("Nome del CSV di training (in Application.persistentDataPath)")]
        public string fileName = "dda_ml_training.csv";

        /// <summary>
        /// If true, writes a single header row with column names
        /// before appending data rows.
        /// </summary>
        [Tooltip("Includi header col nome delle colonne")]
        public bool writeHeader = true;

        private string _filePath;
        private bool _headerWritten;

        /// <summary>
        /// Initialize file path and subscribe to <see cref="DDAController.OnPostEvaluation"/>.
        /// </summary>
        private void Awake()
        {
            if (controller == null)
                controller = FindObjectOfType<DDAController>();

            _filePath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllText(_filePath, string.Empty);

            controller.OnPostEvaluation += HandlePostEvaluation;
        }

        /// <summary>
        /// Unsubscribe from the controller’s event to avoid memory leaks.
        /// </summary>
        private void OnDestroy()
        {
            if (controller != null)
                controller.OnPostEvaluation -= HandlePostEvaluation;
        }

        /// <summary>
        /// Callback invoked after each DDA evaluation.
        /// Writes a header (once) and then one CSV row:
        /// Time; metric1; metric2; …; delta1; delta2; …
        /// </summary>
        /// <param name="time">Current Time.time at evaluation.</param>
        /// <param name="normalized">
        /// Dictionary of metricName → normalizedValue [0–1].</param>
        /// <param name="deltas">
        /// Dictionary of metricName → deltaValue [0–1].</param>
        private void HandlePostEvaluation(
            float time,
            Dictionary<string, float> normalized,
            Dictionary<string, float> deltas)
        {
            if (writeHeader && !_headerWritten)
            {
                WriteHeader(normalized.Keys);
                _headerWritten = true;
            }

            WriteRow(time, normalized, deltas);
        }

        /// <summary>
        /// Writes the CSV header row: "Time;metric1;...;delta1;...".
        /// Metrics and deltas columns use the same metric names.
        /// </summary>
        /// <param name="metricNames">Names of the normalized metrics (and delta metrics).</param>
        private void WriteHeader(IEnumerable<string> metricNames)
        {
            var columns = new List<string> { "Time" };
            columns.AddRange(metricNames);      // normalized metrics
            columns.AddRange(metricNames);      // delta metrics

            File.AppendAllText(
                _filePath,
                string.Join(";", columns) + Environment.NewLine
            );
        }

        /// <summary>
        /// Writes one CSV data row: time + normalized values + delta values,
        /// all formatted to four decimal places.
        /// Uses zero as fallback for any missing delta.
        /// </summary>
        /// <param name="time">The Time.time value for this row.</param>
        /// <param name="normalized">Dictionary of normalized metric values.</param>
        /// <param name="deltas">Dictionary of delta values.</param>
        private void WriteRow(
            float time,
            IDictionary<string, float> normalized,
            IDictionary<string, float> deltas)
        {
            // Start row with the timestamp
            var row = new List<string> { time.ToString("F4") };

            // Append normalized metric values
            foreach (var key in normalized.Keys)
                row.Add(normalized[key].ToString("F4"));

            // Append delta values; fallback to 0.0000 if missing
            foreach (var key in normalized.Keys)
            {
                float value = deltas.TryGetValue(key, out var d) ? d : 0f;
                row.Add(value.ToString("F4"));
            }

            File.AppendAllText(
                _filePath,
                string.Join(";", row) + Environment.NewLine
            );
        }
    }
}
#endif
