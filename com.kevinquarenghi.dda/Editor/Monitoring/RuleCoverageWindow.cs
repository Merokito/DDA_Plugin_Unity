using System.Linq;
using UnityEditor;
using UnityEngine;
using KevinQuarenghi.DDA.Core;          // DDAController, DecisionEngineEvents
using KevinQuarenghi.DDA.Engine;        // FuzzyDecisionEngine, FuzzyRule
using KevinQuarenghi.DDA.Monitoring;    // RuleCoverageTracker

namespace KevinQuarenghi.DDA.Editor.Monitoring
{
    /// <summary>
    /// Finestra per visualizzare, in Play Mode,
    /// quali regole fuzzy si stanno attivando e con quale grado.
    /// </summary>
    public class RuleCoverageWindow : EditorWindow
    {
        private RuleCoverageTracker _tracker;
        private Vector2 _scrollPos;

        [MenuItem("Window/DDA/Rule Coverage")]
        public static void ShowWindow() => GetWindow<RuleCoverageWindow>("Rule Coverage");

        private void OnEnable()
        {
            // Provo a trovare automaticamente un RuleCoverageTracker nella scena
            TryAutoAssignTracker();
        }

        private void OnGUI()
        {
            // Se siamo in Play Mode e non ho un tracker, riprovo a cercarlo
            if (Application.isPlaying && _tracker == null)
                TryAutoAssignTracker();

            EditorGUILayout.HelpBox(
                "Se in scena esiste un RuleCoverageTracker, verrà rilevato automaticamente.",
                MessageType.Info);

            // Campo manuale per assegnazione: opzionale
            _tracker = (RuleCoverageTracker)EditorGUILayout.ObjectField(
                "Tracker (opzionale)", _tracker, typeof(RuleCoverageTracker), true);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Entra in Play Mode per vedere le attivazioni delle regole.",
                    MessageType.Warning);
                return;
            }

            if (_tracker == null)
            {
                EditorGUILayout.HelpBox(
                    "Nessun RuleCoverageTracker valido trovato in scena.",
                    MessageType.Warning);
                return;
            }

            // Trovo il controller per ottenere l'engine e la configurazione
            var controller = FindObjectOfType<DDAController>();
            if (controller == null)
            {
                EditorGUILayout.HelpBox(
                    "Nessun DDAController trovato in scena.",
                    MessageType.Warning);
                return;
            }

            // Cerco un motore fuzzy per poter leggere le regole
            var fuzzyEngine = controller.Engine as FuzzyDecisionEngine;
            if (fuzzyEngine == null)
            {
                EditorGUILayout.HelpBox(
                    "L'engine corrente non è un FuzzyDecisionEngine; niente regole da mostrare.",
                    MessageType.Warning);
                return;
            }

            // Recupero le attivazioni registrate nell'ultimo ciclo
            var activations = _tracker.GetLatestActivations();

            // Inizio scroll
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            // Ordino per grado di attivazione decrescente
            foreach (var kv in activations.OrderByDescending(kv => kv.Value))
            {
                int idx = kv.Key;
                float μ = kv.Value;
                var rule = fuzzyEngine.Config.rules[idx];

                // Descrivo la regola e visualizzo un slider che mostra μ
                EditorGUILayout.LabelField(
                    $"Regola {idx}: {DescribeRule(rule)}",
                    EditorStyles.boldLabel);
                EditorGUILayout.Slider(μ, 0f, 1f);
                EditorGUILayout.Space(8);
            }

            EditorGUILayout.EndScrollView();
        }

        private void OnInspectorUpdate()
        {
            // Aggiorna la finestra ogni frame di Play Mode
            if (Application.isPlaying)
                Repaint();
        }

        /// <summary>
        /// Cerca in scena un RuleCoverageTracker e lo assegna a _tracker.
        /// </summary>
        private void TryAutoAssignTracker()
        {
            _tracker = FindObjectOfType<RuleCoverageTracker>();
        }

        /// <summary>
        /// Costruisce una stringa di testo che descrive
        /// le condizioni e le azioni di una regola fuzzy.
        /// </summary>
        private string DescribeRule(FuzzyRule rule)
        {
            // Scelgo il separatore in base alla presenza di OR
            var sep = rule.conditions.Skip(1).Any(c => c.useOrWithPrev)
                ? " OR "
                : " AND ";
            var conds = string.Join(
                sep,
                rule.conditions.Select(c => $"{c.variable}={c.term}"));
            var acts = string.Join(
                ", ",
                rule.actions.Select(a => $"{a.variable}:{a.term}"));
            return $"{conds} → {acts}";
        }
    }
}
