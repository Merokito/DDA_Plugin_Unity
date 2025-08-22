using System.IO;
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using KevinQuarenghi.DDA.Engine;  
using KevinQuarenghi.DDA.Config;
#endif

#if UNITY_EDITOR
namespace KevinQuarenghi.DDA.Editor.Config
{
    /// <summary>
    /// Utility per importare ed esportare la configurazione fuzzy
    /// tra JSON e ScriptableObject.
    /// </summary>
    public static class FuzzyConfigUtility
    {
        /// <summary>
        /// Menu item per importare la configurazione fuzzy da JSON
        /// e salvarla come ScriptableObject.
        /// </summary>
        [MenuItem("DDA/Config/Fuzzy/Import FuzzyConfig to SO")]
        public static void Import()
        {
            // Seleziona il file JSON di origine
            string jsonFile = EditorUtility.OpenFilePanel(
                "Select FuzzyConfig JSON",
                Application.dataPath,
                "json"
            );
            if (string.IsNullOrEmpty(jsonFile))
                return;

            // Deserializza la configurazione
            string json = File.ReadAllText(jsonFile);
            FuzzyConfig cfg;
            try { cfg = JsonUtility.FromJson<FuzzyConfig>(json); }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DDA] Errore parsing JSON: {ex.Message}");
                return;
            }
            if (cfg == null)
            {
                Debug.LogError("[DDA] Deserializzazione JSON restituisce null");
                return;
            }

            // Scegli dove salvare lo ScriptableObject
            string suggestedName = Path.GetFileNameWithoutExtension(jsonFile) + ".asset";
            string savePath = EditorUtility.SaveFilePanelInProject(
                "Save FuzzyConfig SO",
                suggestedName,
                "asset",
                "Scegli dove salvare il FuzzyConfig ScriptableObject"
            );
            if (string.IsNullOrEmpty(savePath))
                return;

            // Crea e popola l'asset
            var asset = ScriptableObject.CreateInstance<FuzzyConfigSO>();
            asset.variables = cfg.variables;
            asset.rules = cfg.rules;

            // Salva l'asset
            AssetDatabase.CreateAsset(asset, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Seleziona l'asset appena creato
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            Debug.Log($"[DDA] Config fuzzy importata in '{savePath}'.");
        }

        /// <summary>
        /// Menu item per esportare la configurazione fuzzy da uno ScriptableObject
        /// selezionato a JSON. Il file JSON esistente viene sovrascritto,
        /// altrimenti viene creato.
        /// </summary>
        [MenuItem("DDA/Config/Fuzzy/Export FuzzyConfig to JSON")]
        public static void Export()
        {
            // Ottieni il SO selezionato
            var asset = Selection.activeObject as FuzzyConfigSO;
            if (asset == null)
            {
                Debug.LogError("[DDA] Nessun FuzzyConfigSO selezionato per esportazione.");
                return;
            }

            // Chiedi percorso di salvataggio per il JSON
            string defaultName = asset.name + ".json";
            string jsonFile = EditorUtility.SaveFilePanel(
                title: "Save FuzzyConfig JSON",
                directory: Application.dataPath,
                defaultName: defaultName,
                extension: "json"
            );
            if (string.IsNullOrEmpty(jsonFile))
                return;

            // Costruisci un FuzzyConfig in memoria
            var cfg = new FuzzyConfig
            {
                variables = asset.variables,
                rules = asset.rules
            };

            // Serializza e scrivi il file
            try
            {
                string json = JsonUtility.ToJson(cfg, true);
                File.WriteAllText(jsonFile, json);
                Debug.Log($"[DDA] Config fuzzy esportata in '{jsonFile}'.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DDA] Errore scrittura JSON: {ex.Message}");
            }
        }
    }
}
#endif
