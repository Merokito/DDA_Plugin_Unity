using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KevinQuarenghi.DDA.Engine
{
    /// <summary>
    /// Rappresenta la configurazione fuzzy deserializzata da JSON.
    /// I termini di ogni variabile sono espressi in percentuale [0–1].
    /// </summary>
    [Serializable]
    public class FuzzyConfig
    {
        public List<FuzzyVariable> variables;
        public List<FuzzyRule> rules;
    }

    /// <summary>
    /// Definizione di una variabile fuzzy:
    /// nome e lista di termini (Low, Medium, High…) con punti di membership.
    /// </summary>
    [Serializable]
    public class FuzzyVariable
    {
        public string name;
        /// <summary>
        /// Ogni fuzzy term usa points di lunghezza 3 (triangolo) o 4 (trapezio).
        /// </summary>
        public List<FuzzyTerm> terms;
    }

    /// <summary>
    /// Un termine fuzzy definito da un’etichetta e da punti per membership.
    /// </summary>
    [Serializable]
    public class FuzzyTerm
    {
        public string label;
        public float[] points;
    }

    /// <summary>
    /// Una regola fuzzy: condizioni AND e azioni OR.
    /// </summary>
    [Serializable]
    public class FuzzyRule
    {
        public List<Condition> conditions;
        public List<ActionDef> actions;
    }

    [Serializable]
    public class Condition
    {
        public string variable;
        public string term;
        /// <summary>
        /// Se vero, questa condizione viene combinata con la precedente usando OR;
        /// altrimenti viene usato AND. Ignorato per la prima condizione.
        /// </summary>
        public bool useOrWithPrev = false;
    }

    [Serializable]
    public class ActionDef
    {
        public string variable;
        public string term;
    }
}
