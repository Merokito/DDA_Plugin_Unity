using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KevinQuarenghi.DDA.Abstraction;
using KevinQuarenghi.DDA.Config;
using KevinQuarenghi.DDA.Engine;

namespace KevinQuarenghi.DDA.Core
{
    /// <summary>
    /// Coordina il ciclo DDA:
    /// 1. Raccoglie metriche raw e normalizzate.
    /// 2. Chiede al motore decisionale il delta da applicare.
    /// 3. Inoltra i delta agli adjustment handler.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class DDAController : MonoBehaviour
    {
        /// <summary>
        /// ScriptableObject di configurazione del motore (FuzzyConfigSO, MLConfigSO, ecc.).
        /// </summary>
        [Tooltip("Drop-in un SO derivato da DecisionEngineConfigSO")]
        public DecisionEngineConfigSO configAsset;

        /// <summary>
        /// Intervallo (in secondi) tra successive valutazioni DDA;
        /// se 0 valuta ogni frame.
        /// </summary>
        [Tooltip("Intervallo (s) tra valutazioni; 0 = ogni frame")]
        [Min(0f)]
        public float evaluationInterval = 0.5f;

        private IDecisionEngine _engine;
        private ChannelRegistry _registry;
        private List<IAdjustmentHandler> _handlers;
        private float _nextEvaluationTime = 0f;

        /// <summary>
        /// Accesso pubblico al motore decisionale istanziato.
        /// </summary>
        public IDecisionEngine Engine => _engine;

        /// <summary>
        /// Invocato subito dopo ogni Evaluate(norm), con:
        ///  - time: Time.time corrente,
        ///  - normalized metrics,
        ///  - deltas returned by the engine
        /// </summary>
        public event Action<float, Dictionary<string, float>, Dictionary<string, float>> OnPostEvaluation;

        private void Awake()
        {
            _registry = new ChannelRegistry();
            _handlers = FindObjectsOfType<MonoBehaviour>()
                        .OfType<IAdjustmentHandler>()
                        .ToList();

            if (configAsset == null)
                Debug.LogError("[DDA] configAsset non assegnato in Inspector!");

            _engine = DecisionEngineFactory.Create(configAsset);
        }

        private void Update()
        {
            if (evaluationInterval > 0f && Time.time < _nextEvaluationTime)
                return;

            _nextEvaluationTime = Time.time + evaluationInterval;

            _registry.GetAllMetrics(out var raw, out var norm);
            var deltas = _engine.Evaluate(norm);

            OnPostEvaluation?.Invoke(Time.time, norm, deltas);

            foreach (var handler in _handlers)
                handler.ApplyAdjustments(deltas);
        }
    }
}
