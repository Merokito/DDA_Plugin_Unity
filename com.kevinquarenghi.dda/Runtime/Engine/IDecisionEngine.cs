using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KevinQuarenghi.DDA.Engine
{
    /// <summary>
    /// Contratto generico per un engine DDA.
    /// Implementazioni possono essere rule-based, fuzzy o ML.
    /// </summary>
    public interface IDecisionEngine
    {
        /// <summary>
        /// Riceve metriche normalizzate e restituisce un dizionario di delta da applicare.
        /// </summary>
        /// <param name="normalizedMetrics">Metriche in [0–1], key→value.</param>
        /// <returns>Delta normalizzati [0–1], actionKey→deltaValue.</returns>
        Dictionary<string, float> Evaluate(Dictionary<string, float> normalizedMetrics);
    }
}
