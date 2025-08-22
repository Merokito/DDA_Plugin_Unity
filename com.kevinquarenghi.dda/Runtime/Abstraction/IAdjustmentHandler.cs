using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KevinQuarenghi.DDA.Abstraction
{
    /// <summary>
    /// Interfaccia da implementare per ricevere e applicare i delta calcolati dal DDA Controller.
    /// Ogni IAdjustmentHandler traduce i delta in modifiche concrete al gameplay.
    /// </summary>
    public interface IAdjustmentHandler
    {
        /// <summary>
        /// Applica gli aggiustamenti forniti dal DecisionEngine.
        /// </summary>
        /// <param name="deltas">
        /// Dictionary contenente:
        /// - key: nome dell’azione (es. "SpawnRateDelta", "LootQualityDelta")
        /// - value: valore da applicare (positivo o negativo)
        /// </param>
        void ApplyAdjustments(Dictionary<string, float> deltas);
    }
}
