using KevinQuarenghi.DDA.Engine;
using UnityEngine;

namespace KevinQuarenghi.DDA.Config
{
    /// <summary>
    /// Base astratto per tutti i SO di configurazione dei motori decisionali.
    /// Ogni derivato deve implementare <see cref="CreateEngine"/> per
    /// restituire l’istanza di <see cref="IDecisionEngine"/>.
    /// </summary>
    public abstract class DecisionEngineConfigSO : ScriptableObject
    {
        /// <summary>
        /// Istanzia e restituisce il motore decisionale configurato.
        /// </summary>
        /// <returns>Un oggetto che implementa <see cref="IDecisionEngine"/>.</returns>
        public abstract IDecisionEngine CreateEngine();
    }
}
