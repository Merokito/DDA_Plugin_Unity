using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KevinQuarenghi.DDA.Abstraction;

namespace KevinQuarenghi.DDA.Core
{
    /// <summary>
    /// Scopre tutti gli <see cref="IChannel"/> in scena e li invoca per ottenere
    /// sia i valori grezzi che quelli normalizzati in [0–1].
    /// </summary>
    public class ChannelRegistry
    {
        private readonly List<IChannel> _channels;

        /// <summary>
        /// Costruisce un nuovo registro, trovando in scena tutti i componenti
        /// che implementano <see cref="IChannel"/>.
        /// </summary>
        public ChannelRegistry()
        {
            _channels = GameObject.FindObjectsOfType<MonoBehaviour>()
                                  .OfType<IChannel>()
                                  .ToList();
        }

        /// <summary>
        /// Raccoglie tutte le metriche da ogni channel.
        /// </summary>
        /// <param name="rawMetrics">
        /// Dictionary di coppie { chiave → valore grezzo }.
        /// </param>
        /// <param name="normalizedMetrics">
        /// Dictionary di coppie { chiave → valore normalizzato in [0–1] }.
        /// </param>
        public void GetAllMetrics(
            out Dictionary<string, float> rawMetrics,
            out Dictionary<string, float> normalizedMetrics)
        {
            rawMetrics = new Dictionary<string, float>();
            normalizedMetrics = new Dictionary<string, float>();

            foreach (var channel in _channels)
            {
                foreach (var m in channel.GetMetrics())
                {
                    rawMetrics[m.Key] = m.Value;

                    float range = m.Max - m.Min;
                    float norm = range > 0f
                        ? (m.Value - m.Min) / range
                        : 0f;
                    normalizedMetrics[m.Key] = Mathf.Clamp01(norm);
                }
            }
        }
    }
}
