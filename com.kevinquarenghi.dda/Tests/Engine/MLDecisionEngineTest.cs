using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using KevinQuarenghi.DDA.Config;
using KevinQuarenghi.DDA.Engine;

namespace KevinQuarenghi.DDA.Tests.Engine
{
    [TestFixture]
    public class MLDecisionEngineTest
    {
        private MLDecisionEngine _engine;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Creo la config in memoria
            var config = ScriptableObject.CreateInstance<MLConfigSO>();

            // Carico il TextAsset .onnx bytes del modello di test (Identity)
            const string assetPath =
                "Packages/com.kevinquarenghi.dda/Tests/Models/test_model.onnx";
            TextAsset modelAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            Assert.IsNotNull(modelAsset, $"Impossibile trovare il modello in '{assetPath}'");

            // Assegno solo i modelAssets, non più metricNames
            config.modelAssets = new[] { modelAsset };

            // Istanzio l'engine (il costruttore estrarrà automaticamente
            //    inputName e outputName dal modello ONNX)
            _engine = (MLDecisionEngine)config.CreateEngine();
            Assert.IsNotNull(_engine, "MLDecisionEngine non è stato creato correttamente.");
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _engine.Dispose();
        }

        [Test]
        public void Evaluate_IdentityModel_ReturnsSameValue()
        {
            // Poiché il modello Identity usa:
            //   input:  nome dell'input node (es. "input")
            //   output: nome del graph.output (es. "output")
            // MLDecisionEngine avrà letto automaticamente
            // outputName = "output" e inputName = "input".
            //
            // Però, Evaluate si basa su _metricNames (cioè output names)
            // per costruire il buffer delle feature, quindi qui
            // la chiave del dizionario deve essere "output".

            var input = new Dictionary<string, float>
            {
                ["output"] = 0f
            };

            var result = _engine.Evaluate(input);

            // Controllo che venga restituita la chiave "output"
            Assert.IsTrue(result.ContainsKey("output"),
                "Evaluate non ha restituito la chiave 'output'.");

            // Verifica che 0→0 per il modello Identity
            Assert.AreEqual(
                expected: 0f,
                actual: result["output"],
                delta: 1e-6f,
                message: "Per il modello Identity, l'output deve essere uguale all'input 0f."
            );
        }
    }
}
