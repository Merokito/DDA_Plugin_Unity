using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using UnityEngine;
using KevinQuarenghi.DDA.Abstraction;
using KevinQuarenghi.DDA.Config;

namespace KevinQuarenghi.DDA.Engine
{
    /// <summary>
    /// DecisionEngine che utilizza ONNX Runtime per inferenza
    /// di più modelli ONNX, uno per ogni metrica di gioco.
    /// L’utente finale deve solo trascinare i .onnx in MLConfigSO;
    /// tutti i nomi delle metriche e l’ordine di elaborazione
    /// sono gestiti automaticamente.
    /// </summary>
    public class MLDecisionEngine : IDecisionEngine, IDisposable
    {
        private readonly OrtEnv _env;
        private readonly InferenceSession[] _sessions;
        private readonly string[] _metricNames;
        private readonly string _inputName;
        private readonly int _featureCount;

        /// <summary>
        /// Costruisce l’engine ML caricando tutti i modelli da <paramref name="config"/>.
        /// Ordina i modelAssets alfabeticamente per nome e legge automaticamente
        /// i nomi degli output da ciascun modello.
        /// </summary>
        /// <param name="config">
        /// Configurazione ML con:
        /// <list type="bullet">
        ///   <item><description>
        ///     <see cref="MLConfigSO.modelAssets"/>: array di TextAsset .onnx importati
        ///     tramite OnnxScriptedImporter con Base64 dei byte originali.
        ///   </description></item>
        /// </list>
        /// </param>
        public MLDecisionEngine(MLConfigSO config)
        {
            if (config.modelAssets == null || config.modelAssets.Length == 0)
            {
                Debug.LogError("[DDA] MLConfigSO: assegnare almeno un modello ONNX in modelAssets!");
            }

            // Ordina i modelli per nome (es. dda_multi_Difficulty, then _Experience, ...)
            var sortedAssets = config.modelAssets.OrderBy(a => a.name).ToArray();

            _env = OrtEnv.Instance();
            // Crea una sessione per ciascun modello ONNX
            _sessions = sortedAssets
                .Select(asset => new InferenceSession(Convert.FromBase64String(asset.text)))
                .ToArray();

            // Estrae il nome dell'unico input tensor e la dimensione delle feature
            _inputName = _sessions[0].InputMetadata.Keys.First();
            var inputDims = _sessions[0].InputMetadata[_inputName].Dimensions;
            if (inputDims.Length != 2)
            {
                Debug.LogError($"[DDA] Input ONNX inaspettato: attesi 2 assi (batch,features), trovati {inputDims.Length}");
            }
            _featureCount = inputDims[1];

            // Legge automaticamente i nomi degli output (graph.output) da ciascun modello
            _metricNames = _sessions.Select(s => s.OutputMetadata.Keys.First()).ToArray();
            if (_metricNames.Length != _featureCount)
            {
                Debug.LogWarning($"[DDA] Numero modelli ({_metricNames.Length}) diverso da featureCount ({_featureCount})");
            }
        }

        /// <summary>
        /// Esegue l'inferenza su tutti i modelli, creando un unico input tensor [1, featureCount]
        /// e restituendo un dizionario mappa({metrica -> delta predetto}).
        /// </summary>
        /// <param name="normalizedMetrics">
        /// Dizionario nomeMetrica -> valore normalizzato [0..1].
        /// Deve contenere almeno tutte le chiavi presenti in <c>_metricNames</c>.
        /// </param>
        /// <returns>
        /// Dizionario nomeMetrica -> Δ predetto da ciascun modello.
        /// </returns>
        public Dictionary<string, float> Evaluate(Dictionary<string, float> normalizedMetrics)
        {
            // Popola il buffer secondo l'ordine di _metricNames
            var buffer = new float[_featureCount];
            for (int i = 0; i < _featureCount; i++)
            {
                var key = _metricNames[i];
                buffer[i] = normalizedMetrics.TryGetValue(key, out var v) ? v : 0f;
            }

            // Crea un DenseTensor<float> di shape [1, featureCount]
            var inputTensor = new DenseTensor<float>(buffer, new[] { 1, _featureCount });
            var container = NamedOnnxValue.CreateFromTensor(_inputName, inputTensor);

            var result = new Dictionary<string, float>(_sessions.Length);
            // Esegue inferenza su ciascun modello e mappa l'output al nome metrica
            for (int i = 0; i < _sessions.Length; i++)
            {
                using var outputs = _sessions[i].Run(new[] { container });
                var outTensor = outputs.First().AsTensor<float>();
                result[_metricNames[i]] = outTensor.Length > 0 ? outTensor.GetValue(0) : 0f;
            }
            return result;
        }

        /// <summary>
        /// Rilascia le risorse native di ciascuna sessione ONNX Runtime.
        /// </summary>
        public void Dispose()
        {
            foreach (var session in _sessions)
                session.Dispose();
        }
    }
}