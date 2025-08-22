using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KevinQuarenghi.DDA.Monitoring;
using System;

namespace KevinQuarenghi.DDA.Engine
{
    /// <summary>
    /// Implementazione fuzzy del motore DDA, conforme a <see cref="IDecisionEngine"/>.
    /// </summary>
    public class FuzzyDecisionEngine : IDecisionEngine
    {
        private readonly FuzzyConfig _config;
        private readonly FuzzyEngine _engine;

        /// <summary>
        /// Espone l’engine fuzzy interno.
        /// </summary>
        public FuzzyEngine Engine => _engine;

        /// <summary>
        /// Espone fuzzy config.
        /// </summary>
        public FuzzyConfig Config => _config;

        public FuzzyDecisionEngine(FuzzyConfig config)
        {
            _config = config;
            _engine = new FuzzyEngine(_config);
        }

        /// <inheritdoc/>
        public Dictionary<string, float> Evaluate(Dictionary<string, float> normalizedMetrics)
        {
            return _engine.Infer(normalizedMetrics);
        }
    }
}