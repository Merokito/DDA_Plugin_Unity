using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KevinQuarenghi.DDA.Abstraction
{
    /// <summary>
    /// Rappresenta una metrica singola completa di range dinamico.
    /// </summary>
    public struct MetricData
    {
        /// <summary>Chiave univoca della metrica (es. "Health").</summary>
        public string Key;
        /// <summary>Valore corrente della metrica.</summary>
        public float Value;
        /// <summary>Valore minimo possibile (da cui normalizzare).</summary>
        public float Min;
        /// <summary>Valore massimo possibile (da cui normalizzare).</summary>
        public float Max;
    }
}
