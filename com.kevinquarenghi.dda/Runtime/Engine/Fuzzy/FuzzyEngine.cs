using KevinQuarenghi.DDA.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KevinQuarenghi.DDA.Engine
{
    /// <summary>
    /// Motore fuzzy puro che esegue:
    /// 1) fuzzify,
    /// 2) inferenza (AND=min, OR=max),
    /// 3) defuzzificazione (centroide sul dominio [0–1]),
    /// e notifica quali regole si attivano
    /// </summary>
    public class FuzzyEngine
    {
        private readonly FuzzyConfig _config;

        /// <summary>
        /// Costruisce il motore a partire da una configurazione fuzzy.
        /// </summary>
        public FuzzyEngine(FuzzyConfig config)
        {
            _config = config;
        }

        /// <summary>
        /// Esegue l’intero processo fuzzy (fuzzificazione, inferenza, defuzzificazione).
        /// </summary>
        /// <param name="inputs">
        /// Dizionario delle metriche normalizzate [0–1],
        /// dove la chiave è il nome della variabile e il valore è il suo input normalizzato.
        /// </param>
        /// <returns>
        /// Dizionario dei delta normalizzati [0–1] per ciascuna variabile di azione,
        /// ottenuti tramite il metodo del centroide sulle uscite fuzzy.
        /// </returns>
        public Dictionary<string, float> Infer(Dictionary<string, float> inputs)
        {
            // 1. Fuzzificazione
            var fuzzified = new Dictionary<(string var, string term), float>();
            foreach (var variable in _config.variables)
            {
                if (!inputs.TryGetValue(variable.name, out var x))
                    continue;
                foreach (var term in variable.terms)
                    fuzzified[(variable.name, term.label)] = FuzzifyTerm(x, term.points);
            }

            // 2. Valutazione regole con tracking delle variabili attivate
            var actionTermActivations = new Dictionary<(string var, string term), List<float>>();
            for (int i = 0; i < _config.rules.Count; i++)
            {
                var rule = _config.rules[i];
                float activation = 0f;
                bool first = true;

                // AND/OR sequenziali tra condizioni
                foreach (var cond in rule.conditions)
                {
                    fuzzified.TryGetValue((cond.variable, cond.term), out var μ);
                    if (first)
                    {
                        activation = μ;
                        first = false;
                    }
                    else if (cond.useOrWithPrev)
                        activation = Mathf.Max(activation, μ);
                    else
                        activation = Mathf.Min(activation, μ);
                }

                // Se la regola ha attivazione positiva, invia evento e accumula le azioni
                if (activation > 0f)
                {
                    DecisionEngineEvents.NotifyRuleActivated(i, activation);

                    foreach (var act in rule.actions)
                    {
                        var key = (act.variable, act.term);
                        if (!actionTermActivations.TryGetValue(key, out var list))
                        {
                            list = new List<float>();
                            actionTermActivations[key] = list;
                        }
                        list.Add(activation);
                    }
                }
            }

            // 3. Costruzione degli output fuzzy solo per termini con α > 0
            var fuzzyTermOutput = new Dictionary<(string var, string term), float>();
            foreach (var kv in actionTermActivations)
            {
                float alphaMax = kv.Value.Max();
                if (alphaMax > 0f)
                    fuzzyTermOutput[kv.Key] = alphaMax;
            }

            // 4. Defuzzificazione solo delle variabili effettivamente attivate
            var crispOutput = new Dictionary<string, float>();
            var triggeredVars = fuzzyTermOutput
                .Keys
                .Select(k => k.var)
                .Distinct();

            foreach (var varName in triggeredVars)
            {
                var variableConfig = _config.variables
                    .First(v => v.name == varName);

                // raccolgo le α dei termini attivati
                var termAlphas = fuzzyTermOutput
                    .Where(kv => kv.Key.var == varName)
                    .ToDictionary(kv => kv.Key.term, kv => kv.Value);

                float defuzzed = Defuzzify(variableConfig.terms, termAlphas);
                crispOutput[varName] = defuzzed;
            }

            return crispOutput;
        }

        /// <summary>
        /// Calcola il grado di appartenenza di un valore x in [0–1]
        /// rispetto a una funzione di membership definita da 3 o 4 punti.
        /// </summary>
        /// <param name="x">Valore normalizzato di input.</param>
        /// <param name="p">
        /// Array di punti:
        ///  - 3 elementi: triangolo [a,b,c]
        ///  - 4 elementi: trapezio [a,b,c,d]
        /// </param>
        /// <returns>Grado di appartenenza μ(x) ∈ [0–1].</returns>
        private float FuzzifyTerm(float x, float[] p)
        {
            // triangolo [a,b,c] o trapezio [a,b,c,d]
            if (p.Length == 3)
            {
                var a = p[0];
                var b = p[1];
                var c = p[2];

                // Shoulder sinistro (a==b): plateau completo fino a b
                if (Mathf.Approximately(a, b))
                {
                    if (x <= b) return 1f;                   // tutto sotto b ha membership 1
                    if (x >= c) return 0f;                   // tutto sopra c è 0
                    return (c - x) / (c - b);                // decresce linearmente da b→c
                }
                // Shoulder destro (b==c): plateau completo da b in poi
                if (Mathf.Approximately(b, c))
                {
                    if (x >= b) return 1f;                   // tutto sopra b ha membership 1
                    if (x <= a) return 0f;                   // tutto sotto a è 0
                    return (x - a) / (b - a);                // cresce linearmente da a→b
                }
                // Triangolo “normale”
                if (x <= a || x >= c) return 0f;
                if (Mathf.Approximately(x, b)) return 1f;
                return x < b
                    ? (x - a) / (b - a)
                    : (c - x) / (c - b);
            }

            if (p.Length == 4)
            {
                var a = p[0];
                var b = p[1];
                var c = p[2];
                var d = p[3];

                // Trapezio con shoulder sinistro
                if (Mathf.Approximately(a, b))
                {
                    if (x <= b) return 1f;
                    if (x >= d) return 0f;
                    if (x <= c) return 1f;                    // plateau piatto tra b e c
                    return (d - x) / (d - c);
                }
                // Trapezio con shoulder destro
                if (Mathf.Approximately(c, d))
                {
                    if (x >= c) return 1f;
                    if (x <= a) return 0f;
                    if (x >= b) return 1f;                    // plateau piatto tra b e c
                    return (x - a) / (b - a);
                }
                // Trapezio “normale”
                if (x <= a || x >= d) return 0f;
                if (x >= b && x <= c) return 1f;
                return x < b
                    ? (x - a) / (b - a)
                    : (d - x) / (d - c);
            }

            return 0f;
        }


        /// <summary>
        /// Approssima il calcolo del centroide su [0–1] per una data variabile fuzzy,
        /// basandosi sui gradi di attivazione di ciascun termine.
        /// </summary>
        /// <param name="terms">Lista dei termini fuzzy con le loro membership functions.</param>
        /// <param name="fuzzyOutput">
        /// Dizionario term→α, dove α è il grado di attivazione massimo per quel termine.
        /// </param>
        /// <returns>
        /// Valore “crisp” ∈ [0–1], risultato della media pesata (centroide).
        /// </returns>
        private float Defuzzify(List<FuzzyTerm> terms, Dictionary<string, float> fuzzyOutput)
        {
            const int STEPS = 50;
            float numerator = 0f, denominator = 0f;

            for (int i = 0; i <= STEPS; i++)
            {
                float z = (float)i / STEPS;
                float μz = 0f;
                // μ(z) = max per ogni term di min(activation, membership(z))
                foreach (var term in terms)
                {
                    if (!fuzzyOutput.TryGetValue(term.label, out var α))
                        continue;
                    var μt = FuzzifyTerm(z, term.points);
                    μz = Mathf.Max(μz, Mathf.Min(α, μt));
                }
                numerator += μz * z;
                denominator += μz;
            }
            return denominator > 0f ? numerator / denominator : 0f;
        }
    }
}