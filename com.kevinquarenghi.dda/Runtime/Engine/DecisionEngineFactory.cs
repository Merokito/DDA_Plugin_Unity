using KevinQuarenghi.DDA.Config;

namespace KevinQuarenghi.DDA.Engine
{
    /// <summary>
    /// Factory leggera per creare istanze di <see cref="IDecisionEngine"/>
    /// a partire da qualsiasi <see cref="DecisionEngineConfigSO"/>.
    /// Permette di centralizzare eventuali decoratori, caching o logging.
    /// </summary>
    public static class DecisionEngineFactory
    {
        /// <summary>
        /// Crea un motore decisionale basato sui parametri specificati
        /// nel <paramref name="config"/> passato.
        /// </summary>
        /// <param name="config">ScriptableObject di configurazione.</param>
        /// <returns>Nuova istanza di <see cref="IDecisionEngine"/>.</returns>
        public static IDecisionEngine Create(DecisionEngineConfigSO config)
        {
            if (config == null)
                throw new System.ArgumentNullException(nameof(config));
            return config.CreateEngine();
        }

        /// <summary>
        /// (DEPRECATO) Crea un motore fuzzy a partire da un file JSON.
        /// </summary>
        /// <param name="configPath">Percorso del file JSON.</param>
        /// <returns>Istancea un <see cref="FuzzyDecisionEngine"/>.</returns>
        [System.Obsolete("Usa lo ScriptableObject invece del file path")]
        public static IDecisionEngine Create(string configPath)
        {
            if (!System.IO.File.Exists(configPath))
                throw new System.IO.FileNotFoundException("Config file non trovato", configPath);

            string json = System.IO.File.ReadAllText(configPath);
            var cfg = UnityEngine.JsonUtility.FromJson<FuzzyConfig>(json);
            return new FuzzyDecisionEngine(cfg);
        }
    }
}
