using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KevinQuarenghi.DDA.Core;       // ChannelRegistry

namespace KevinQuarenghi.DDA.Monitoring
{
    /// <summary>
    /// Componente che, a intervalli discreti, preleva dal <see cref="ChannelRegistry"/>
    /// le metriche normalizzate e popol­a i corrispondenti <see cref="MetricSeries"/>.
    /// Il dev inserisce in Inspector tutti gli asset <see cref="MetricSeries"/>
    /// da monitorare (il campo <c>metricName</c> deve corrispondere alla chiave
    /// restituita dal registry, es. "Health", "SpawnRate", ecc.).
    /// </summary>
    [DisallowMultipleComponent]
    public class MetricsMonitor : MonoBehaviour
    {
        /*********************************************************************
         * CONFIGURAZIONE SAMPLING
         *********************************************************************/

        /// <summary>
        /// Intervallo (in secondi) tra un campionamento e il successivo.
        /// Se 0, campiona ogni frame.
        /// </summary>
        [Tooltip("Intervallo tra i campionamenti (in secondi); se 0 campiona ogni frame.")]
        [Min(0f)]
        public float samplingInterval = 0.1f;

        /// <summary>
        /// Timestamp (Time.time) del prossimo campionamento consentito.
        /// </summary>
        private float _nextSampleTime = 0f;

        /*********************************************************************
         * ASSET DA MONITORARE
         *********************************************************************/

        [Header("Metric Series da popolare (metricName deve corrispondere)")]
        [Tooltip("Drag & drop qui tutti i MetricSeries che volete monitorare")]
        public List<MetricSeries> seriesAssets;

        /// <summary>
        /// Mappa interna da nome metrica a <see cref="MetricSeries"/>.
        /// </summary>
        private Dictionary<string, MetricSeries> _seriesMap;

        /// <summary>
        /// Registro delle metriche, da cui leggiamo raw e normalized.
        /// </summary>
        private ChannelRegistry _registry;

        /*********************************************************************
         * CICLO DI VITA UNITY
         *********************************************************************/

        private void Awake()
        {
            _registry = new ChannelRegistry();

            // costruisco mappa solo per asset validi
            _seriesMap = seriesAssets
                .Where(s => !string.IsNullOrEmpty(s.metricName))
                .ToDictionary(s => s.metricName, s => s);

            // resetto tutte le serie
            foreach (var series in _seriesMap.Values)
                series.Clear();

            // imposto il primo campionamento
            _nextSampleTime = Time.time;
        }

        private void Update()
        {
            // se samplingInterval > 0, aspetto il prossimo turno
            if (samplingInterval > 0f && Time.time < _nextSampleTime)
                return;

            // aggiorno il timer per il prossimo campionamento
            _nextSampleTime = Time.time + samplingInterval;

            // 1) estraggo metriche raw e normalized
            _registry.GetAllMetrics(out var rawMetrics, out var normalizedMetrics);

            // 2) timestamp corrente
            float now = Time.time;

            // 3) aggiungo il sample solo alle serie corrispondenti
            foreach (var kv in normalizedMetrics)
            {
                if (_seriesMap.TryGetValue(kv.Key, out var series))
                {
                    series.AddSample(now, kv.Value);
                }
            }
        }
    }

}