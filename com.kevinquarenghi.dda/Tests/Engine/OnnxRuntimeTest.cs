using System.IO;
using System.Linq;
using NUnit.Framework;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using UnityEngine;

namespace KevinQuarenghi.DDA.Tests.Engine
{
    /// <summary>
    /// Test fixture che verifica sia il caricamento di un modello ONNX
    /// (rinominato in .bytes) da file sia l’esecuzione di una inferenza minima
    /// tramite ONNX Runtime.  
    /// Il file di test deve chiamarsi "test_model.onnx.bytes" e trovarsi in
    /// Packages/com.kevinquarenghi.dda/Tests/Models/.
    /// </summary>
    [TestFixture]
    public class OnnxRuntimeTest
    {
        private OrtEnv _env;
        private InferenceSession _session;
        private string _modelPathBytes;

        /// <summary>
        /// One‐time setup:
        /// 1) calcola la root del progetto
        /// 2) costruisce il path a "test_model.onnx.bytes"
        /// 3) verifica che esista
        /// 4) legge i byte
        /// 5) inizializza OrtEnv e crea InferenceSession(byte[])
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // 1) cartella root (quella che contiene Assets/)
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

            // 2) percorso al file .bytes dentro Tests/Models
            _modelPathBytes = Path.Combine(
                projectRoot,
                "Packages",
                "com.kevinquarenghi.dda",
                "Tests",
                "Models",
                "test_model.onnx"
            );

            // 3) verifica che esista
            Assert.IsTrue(
                File.Exists(_modelPathBytes),
                $"Il file di test ONNX (.bytes) non è stato trovato: {_modelPathBytes}"
            );

            // 4) legge i byte
            byte[] modelBytes = File.ReadAllBytes(_modelPathBytes);

            // 5) inizializza ambiente e sessione ONNX Runtime
            _env = OrtEnv.Instance();
            _session = new InferenceSession(modelBytes);
        }

        /// <summary>
        /// One‐time teardown: rilascia la sessione.
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _session.Dispose();
        }

        /// <summary>
        /// Verifica che la sessione sia stata creata correttamente.
        /// </summary>
        [Test]
        public void Session_Creation_DoesNotThrow()
        {
            Assert.IsNotNull(_session, "Sessione ONNX Runtime non creata.");
        }

        /// <summary>
        /// Costruisce un tensore di zeri per il primo input, esegue <c>Run</c>
        /// e verifica che il modello restituisca almeno un valore in output.
        /// </summary>
        [Test]
        public void Inference_ReturnsExpectedOutput()
        {
            // nome del primo input/output
            string inputName = _session.InputMetadata.Keys.First();
            string outputName = _session.OutputMetadata.Keys.First();

            // dimensioni, sostituendo -1 con 1
            int[] dims = _session.InputMetadata[inputName]
                .Dimensions
                .Select(d => d > 0 ? d : 1)
                .ToArray();

            // array di zeri + DenseTensor
            int totalSize = dims.Aggregate(1, (prod, d) => prod * d);
            var buffer = new float[totalSize];
            var tensor = new DenseTensor<float>(buffer, dims);

            // NamedOnnxValue
            var input = NamedOnnxValue.CreateFromTensor(inputName, tensor);

            using var results = _session.Run(new[] { input });

            // sanity checks
            Assert.IsNotNull(results, "Risultati inferenza nulli.");
            Assert.IsTrue(results.Any(), "Nessun output restituito.");

            // estrai tensor e verifica contenuto
            var outTensor = results.First(r => r.Name == outputName).AsTensor<float>();
            Assert.Greater(
                outTensor.Length,
                0,
                $"Output '{outputName}' è vuoto."
            );
        }
    }
}
