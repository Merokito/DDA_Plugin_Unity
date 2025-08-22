using System.Collections.Generic;
using UnityEngine;
using KevinQuarenghi.DDA.Core;

namespace KevinQuarenghi.DDA.Monitoring
{
    /// <summary>
    /// Monitora in tempo reale quali regole del motore DDA
    /// si attivano, utilizzando il bus di eventi <see cref="DecisionEngineEvents"/>.
    /// Ogni volta che comincia un nuovo ciclo di valutazione (identificato
    /// dal cambio di frame), le vecchie attivazioni vengono cancellate,
    /// così da mantenere in memoria solo le regole effettivamente attive
    /// in quell’istante.
    /// </summary>
    [DisallowMultipleComponent]
    public class RuleCoverageTracker : MonoBehaviour
    {
        /// <summary>
        /// Mappa (indiceRegola → grado μ) delle attivazioni registrate
        /// nel ciclo corrente.
        /// </summary>
        private readonly Dictionary<int, float> _latestActivations = new Dictionary<int, float>();

        /// <summary>
        /// Frame in cui è stata registrata l’ultima attivazione.
        /// Usato per capire quando inizia un nuovo ciclo.
        /// </summary>
        private int _lastRecordedFrame = -1;

        /// <summary>
        /// Al momento dell’abilitazione, si iscrive
        /// all’evento <see cref="DecisionEngineEvents.RuleActivated"/>.
        /// </summary>
        private void OnEnable()
        {
            DecisionEngineEvents.RuleActivated += RecordActivation;
        }

        /// <summary>
        /// Alla disabilitazione, si desiscrive dall’evento.
        /// </summary>
        private void OnDisable()
        {
            DecisionEngineEvents.RuleActivated -= RecordActivation;
        }

        /// <summary>
        /// Callback invocata ogni volta che una regola si attiva (μ > 0).
        /// Se è la prima attivazione del ciclo (frame) corrente, pulisce
        /// la mappa delle vecchie attivazioni prima di registrare la nuova.
        /// </summary>
        /// <param name="ruleIndex">Indice della regola nel configuration asset.</param>
        /// <param name="activation">Grado di attivazione (μ) della regola.</param>
        private void RecordActivation(int ruleIndex, float activation)
        {
            int currentFrame = Time.frameCount;

            if (currentFrame != _lastRecordedFrame)
            {
                _latestActivations.Clear();
                _lastRecordedFrame = currentFrame;
            }

            // Registro o aggiorno il grado di attivazione
            _latestActivations[ruleIndex] = activation;
        }

        /// <summary>
        /// Restituisce le attivazioni registrate per il ciclo corrente:
        /// regola → grado μ.
        /// </summary>
        public IReadOnlyDictionary<int, float> GetLatestActivations()
        {
            return _latestActivations;
        }
    }
}
