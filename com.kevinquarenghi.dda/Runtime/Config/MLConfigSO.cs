using KevinQuarenghi.DDA.Abstraction;
using KevinQuarenghi.DDA.Engine;
using UnityEngine;

namespace KevinQuarenghi.DDA.Config
{
    /// <summary>
    /// ScriptableObject per la configurazione dell'engine ML.
    /// Contiene l'array di TextAsset .onnx da inferire.
    /// L'ordine e i nomi delle metriche sono ricavati automaticamente.
    /// </summary>
    [CreateAssetMenu(
        menuName = "DDA/Config/MLConfigSO",
        fileName = "MLConfigSO")]
    public class MLConfigSO : DecisionEngineConfigSO
    {
        /*********************************************************************
         * MODEL ASSETS
         *********************************************************************/

        /// <summary>
        /// Array di asset <see cref="TextAsset"/> contenenti i modelli .onnx.
        /// L'ordine di elaborazione viene determinato alfabeticamente dal nome
        /// del file (es. "dda_multi_Difficulty", "dda_multi_Experience", ...).
        /// </summary>
        [Tooltip("TextAsset .onnx: trascina qui tutti i modelli da inferire")]
        public TextAsset[] modelAssets;

        /*********************************************************************
         * ENGINE CREATION
         *********************************************************************/

        /// <inheritdoc/>
        /// <remarks>
        /// Crea un'istanza di <see cref="KevinQuarenghi.DDA.Engine.MLDecisionEngine"/>
        /// utilizzando solo <c>modelAssets</c>; non serve alcun array di nomi metriche.
        /// </remarks>
        public override IDecisionEngine CreateEngine()
        {
            return new MLDecisionEngine(this);
        }
    }
}
