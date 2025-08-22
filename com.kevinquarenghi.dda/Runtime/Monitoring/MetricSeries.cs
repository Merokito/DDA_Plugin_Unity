using System.Collections.Generic;
using UnityEngine;

namespace KevinQuarenghi.DDA.Monitoring
{
    /// <summary>
    /// ScriptableObject che rappresenta una serie temporale di campioni
    /// per una metrica normalizzata [0–1].  
    /// Viene salvata come asset e popolata in Play Mode attraverso chiamate a <see cref="AddSample"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "DDA/Monitoring/Metric Series", fileName = "NewMetricSeries")]
    public class MetricSeries : ScriptableObject
    {
        /*********************************************************************
         * CONFIGURAZIONE METRICA
         *********************************************************************/

        /// <summary>
        /// Nome descrittivo della metrica
        /// (ad esempio "Health", "CompletionTime", "SpawnRate").
        /// </summary>
        [Tooltip("Nome della metrica (es. \"Health\").")]
        public string metricName;

        /// <summary>
        /// Valore minimo del band di flow (range [0–1]).
        /// Valori al di sotto di questa soglia sono considerati fuori flow.
        /// </summary>
        [Tooltip("Lower bound del band di flow in [0–1].")]
        [Range(0f, 1f)]
        public float lowerBound = 0.4f;

        /// <summary>
        /// Valore massimo del band di flow (range [0–1]).
        /// Valori al di sopra di questa soglia sono considerati fuori flow.
        /// </summary>
        [Tooltip("Upper bound del band di flow in [0–1].")]
        [Range(0f, 1f)]
        public float upperBound = 0.6f;

        /*********************************************************************
        * CONFIGURAZIONE DEL BUFFER
        *********************************************************************/

        /// <summary>
        /// Numero massimo di campioni da mantenere in memoria.
        /// Quando si supera, i più vecchi vengono scartati.
        /// </summary>
        [Tooltip("Numero massimo di campioni da mantenere.")]
        [Min(10)]
        public int maxSamples = 500;

        /*********************************************************************
         * DATI DI SERIE TEMPORALE
         *********************************************************************/

        /// <summary>
        /// Lista dei timestamp (in secondi) in cui sono stati raccolti i campioni.
        /// Deve avere la stessa lunghezza di <see cref="values"/>.
        /// </summary>
        [Tooltip("Timestamps (in secondi) dei campioni raccolti.")]
        public List<float> timestamps = new List<float>();

        /// <summary>
        /// Lista dei valori corrispondenti ai timestamp in <see cref="timestamps"/>.
        /// I valori devono essere normalizzati nell'intervallo [0–1].
        /// </summary>
        [Tooltip("Valori della metrica raccolti (normalizzati 0–1).")]
        public List<float> values = new List<float>();

        /*********************************************************************
         * METODI DI GESTIONE DELLA SERIE
         *********************************************************************/

        /// <summary>
        /// Aggiunge un nuovo campione alla serie temporale.
        /// Scarta il campione più vecchio se si supera <see cref="maxSamples"/>.
        /// </summary>
        /// <param name="time">
        /// Tempo (in secondi dall’avvio del Play Mode)
        /// in cui è avvenuto il campionamento.
        /// </param>
        /// <param name="value">
        /// Valore della metrica rilevato in quel momento (0–1).
        /// </param>
        public void AddSample(float time, float value)
        {
            timestamps.Add(time);
            values.Add(value);
            if (timestamps.Count > maxSamples)
            {
                // scarta il campione più vecchio
                timestamps.RemoveAt(0);
                values.RemoveAt(0);
            }
        }

        /// <summary>
        /// Rimuove tutti i campioni dalla serie.
        /// Utile da richiamare in <c>Awake()</c> o <c>OnEnable()</c>
        /// per inizializzare una nuova sessione di Play Mode.
        /// </summary>
        public void Clear()
        {
            timestamps.Clear();
            values.Clear();
        }

        /// <summary>
        /// Numero corrente di campioni memorizzati nella serie.
        /// </summary>
        public int Count => timestamps.Count;

        /*********************************************************************
         * METODI DI UTILITÀ 
         *********************************************************************/

        /// <summary>
        /// Restituisce l’ultimo valore campionato o 0 se la serie è vuota.
        /// </summary>
        public float LastValue => Count > 0 ? values[Count - 1] : 0f;

        /// <summary>
        /// Restituisce l’ultimo timestamp campionato o 0 se la serie è vuota.
        /// </summary>
        public float LastTime => Count > 0 ? timestamps[Count - 1] : 0f;

        /// <summary>
        /// Ritorna la percentuale di campioni correnti che ricadono
        /// all’interno del band di flow [<see cref="lowerBound"/>–<see cref="upperBound"/>].
        /// </summary>
        public float FlowRatio()
        {
            if (Count == 0) return 0f;
            int inFlow = 0;
            foreach (var v in values)
            {
                if (v >= lowerBound && v <= upperBound)
                    inFlow++;
            }
            return (float)inFlow / Count;
        }
    }
}
