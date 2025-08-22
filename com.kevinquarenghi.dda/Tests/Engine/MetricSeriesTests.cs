using NUnit.Framework;
using UnityEngine;
using KevinQuarenghi.DDA.Monitoring;
using System.Linq;

namespace KevinQuarenghi.DDA.Tests.Engine
{
    public class MetricSeriesTests
    {
        private MetricSeries _series;

        [SetUp]
        public void Setup()
        {
            // crea un'istanza vuota in memoria
            _series = ScriptableObject.CreateInstance<MetricSeries>();
            _series.metricName = "TestMetric";
        }

        [TearDown]
        public void Teardown()
        {
            // distrugge l'istanza per pulire la memoria
            Object.DestroyImmediate(_series);
        }

        [Test]
        public void NewSeries_IsEmpty()
        {
            Assert.AreEqual(0, _series.Count, "Count iniziale deve essere 0");
            Assert.IsEmpty(_series.timestamps, "timestamps deve essere vuoto");
            Assert.IsEmpty(_series.values, "values deve essere vuoto");
        }

        [Test]
        public void AddSample_AppendsTimestampAndValue()
        {
            _series.AddSample(1.23f, 4.56f);

            Assert.AreEqual(1, _series.Count, "Count dopo un AddSample deve essere 1");
            Assert.AreEqual(1.23f, _series.timestamps[0], 0.0001f, "Timestamp memorizzato errato");
            Assert.AreEqual(4.56f, _series.values[0], 0.0001f, "Value memorizzato errato");

            // aggiungo un secondo campione
            _series.AddSample(2.0f, 7.0f);
            Assert.AreEqual(2, _series.Count, "Count dopo due AddSample deve essere 2");

            // controllo che le liste siano parallele
            Assert.IsTrue(_series.timestamps.SequenceEqual(new[] { 1.23f, 2.0f }));
            Assert.IsTrue(_series.values.SequenceEqual(new[] { 4.56f, 7.0f }));
        }

        [Test]
        public void Clear_EmptiesAllSamples()
        {
            _series.AddSample(0f, 1f);
            _series.AddSample(1f, 2f);
            Assert.AreEqual(2, _series.Count);

            _series.Clear();
            Assert.AreEqual(0, _series.Count);
            Assert.IsEmpty(_series.timestamps);
            Assert.IsEmpty(_series.values);
        }
    }
}
