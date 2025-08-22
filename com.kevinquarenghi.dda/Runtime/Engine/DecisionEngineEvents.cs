using System;

namespace KevinQuarenghi.DDA.Core
{
    /// <summary>
    /// Event Aggregator per le notifiche di attivazione regole
    /// da qualunque motore che implementa IDecisionEngine.
    /// </summary>
    public static class DecisionEngineEvents
    {
        /// <summary>
        /// Regola attivata: (indiceRegola, grado μ)
        /// </summary>
        public static event Action<int, float> RuleActivated;

        /// <summary>
        /// Metodo interno per pubblicare l’evento.
        /// </summary>
        public static void NotifyRuleActivated(int index, float μ)
            => RuleActivated?.Invoke(index, μ);
    }
}
