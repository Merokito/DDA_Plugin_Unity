using System.IO;
using UnityEditor.AssetImporters;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

/// <summary>
/// ScriptedImporter che permette a Unity di trattare i file �.onnx� come TextAsset,
/// esponendo i byte grezzi del modello per i tuoi test e per il MLDecisionEngine.
/// </summary>
[ScriptedImporter(version: 1, ext: "onnx")]
public class OnnxScriptedImporter : ScriptedImporter
{
    /// <summary>
    /// Chiamato da Unity ogni volta che un asset �.onnx� viene aggiunto o modificato.
    /// </summary>
    /// <param name="ctx">
    /// Contesto di importazione fornito da Unity, contiene informazioni sul percorso
    /// e permette di registrare gli oggetti creati.
    /// </param>
    public override void OnImportAsset(AssetImportContext ctx)
    {
        // Legge tutti i byte del file .onnx dal disco
        byte[] modelBytes = File.ReadAllBytes(ctx.assetPath);

        // Crea un TextAsset che contiene i byte del modello (base64 per serializzazione in asset)
        var textAsset = new TextAsset(System.Convert.ToBase64String(modelBytes))
        {
            name = Path.GetFileNameWithoutExtension(ctx.assetPath)
        };

        // Aggiunge il TextAsset all�asset bundle di importazione
        ctx.AddObjectToAsset("ModelBytes", textAsset);

        // Imposta il TextAsset come oggetto principale dell�importazione
        ctx.SetMainObject(textAsset);
    }
}
