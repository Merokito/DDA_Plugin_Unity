#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;

/// <summary>
/// Gestisce l�importazione del plugin ONNX Runtime in Unity.
/// Disabilita automaticamente la compatibilit� con l�Editor 
/// per la DLL vendorizzata di ONNX Runtime non appena viene importata o ricompilata.
/// </summary>
[InitializeOnLoad]
internal static class OnnxRuntimePluginImporter
{
    // Path relativo alla DLL gestita da questo importer.
    private const string PluginPath = "Packages/com.kevinquarenghi.dda/Plugins/Managed/Microsoft.ML.OnnxRuntime.dll";

    /// <summary>
    /// Costruttore statico: si registra all�evento di fine compilazione
    /// non appena l�editor carica questa classe.
    /// </summary>
    static OnnxRuntimePluginImporter()
    {
        CompilationPipeline.compilationFinished += DisableInEditor;
    }

    /// <summary>
    /// Disabilita la compatibilit� della DLL ONNX Runtime con l�Editor.
    /// Viene chiamato ad ogni compilazione e import iniziale.
    /// </summary>
    /// <param name="context">
    /// Contesto della compilazione (ignorato).
    /// </param>
    private static void DisableInEditor(object context)
    {
        var importer = AssetImporter.GetAtPath(PluginPath) as PluginImporter;
        if (importer == null)
            return;

        // Se la DLL � ancora marcata come �compatibile con l�Editor�
        if (importer.GetCompatibleWithEditor())
        {
            // disabilitiamo
            importer.SetCompatibleWithEditor(false);
            // Forziamo un reimport per applicare subito il cambiamento
            AssetDatabase.ImportAsset(PluginPath, ImportAssetOptions.ForceUpdate);
        }
    }
}
#endif
