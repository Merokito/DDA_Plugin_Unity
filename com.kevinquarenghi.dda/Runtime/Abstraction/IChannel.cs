using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KevinQuarenghi.DDA.Abstraction
{
    /// <summary>
    /// Interfaccia per esporre metriche di gioco al DDA Controller.
    /// Ogni channel restituisce una lista di <see cref="MetricData"/> contenenti
    /// valore grezzo e range (min/max) corrente.
    /// </summary>
    public interface IChannel
    {
        /// <summary>
        /// Ottiene le metriche correnti di questo channel.
        /// </summary>
        /// <returns>
        /// Lista di <see cref="MetricData"/>, in cui:
        /// - <c>Key</c> è il nome della metrica (es. "Health").
        /// - <c>Value</c> è il valore grezzo corrente.
        /// - <c>Min</c> e <c>Max</c> definiscono il range attuale per la normalizzazione.
        /// </returns>
        IReadOnlyList<MetricData> GetMetrics();
    }
}
