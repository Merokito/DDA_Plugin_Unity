// Assets/KeQuarenghi/DDA/Config/FuzzyConfigSO.cs
using KevinQuarenghi.DDA.Engine;
using System.Collections.Generic;
using UnityEngine;

namespace KevinQuarenghi.DDA.Config
{
    /// <summary>
    /// ScriptableObject che definisce la configurazione per l’engine fuzzy.
    /// Contiene la lista di variabili fuzzy e le regole IF→THEN da applicare.
    /// </summary>
    [CreateAssetMenu(menuName = "DDA/Config/FuzzyConfigSO", fileName = "FuzzyConfigSO")]
    public class FuzzyConfigSO : DecisionEngineConfigSO
    {
        /// <summary>
        /// Collezione di variabili fuzzy (input e output), ciascuna con i suoi termini.
        /// </summary>
        [Tooltip("Variabili fuzzy da utilizzare nell'inferenza")]
        public List<FuzzyVariable> variables;

        /// <summary>
        /// Insieme di regole fuzzy che combinano condizioni e azioni.
        /// </summary>
        [Tooltip("Regole fuzzy")]
        public List<FuzzyRule> rules;

        /// <inheritdoc/>
        public override IDecisionEngine CreateEngine()
        {
            // Converte il SO in un FuzzyConfig utilizzabile dal motore
            var cfg = new FuzzyConfig
            {
                variables = new List<FuzzyVariable>(variables),
                rules = new List<FuzzyRule>(rules)
            };
            return new FuzzyDecisionEngine(cfg);
        }
    }
}
