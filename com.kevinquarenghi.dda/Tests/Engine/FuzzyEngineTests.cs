using NUnit.Framework;
using System.Collections.Generic;
using KevinQuarenghi.DDA.Engine;

namespace KevinQuarenghi.DDA.Tests.Engine
{
    [TestFixture]
    public class FuzzyEngineTests
    {
        private FuzzyEngine _engine;

        [SetUp]
        public void SetUp()
        {
            var config = new FuzzyConfig
            {
                variables = new List<FuzzyVariable>
                {
                    new FuzzyVariable {
                        name = "Health",
                        terms = new List<FuzzyTerm> {
                            new FuzzyTerm { label="Low", points=new float[]{0f,0f,0.5f} },
                            new FuzzyTerm { label="High", points=new float[]{0.5f,1f,1f} }
                        }
                    },
                    new FuzzyVariable {
                        name = "SpawnRate",
                        terms = new List<FuzzyTerm> {
                            new FuzzyTerm { label="Low", points=new float[]{0f,0f,0.5f} },
                            new FuzzyTerm { label="High", points=new float[]{0.5f,1f,1f} }
                        }
                    }
                },
                rules = new List<FuzzyRule> {
                    new FuzzyRule {
                        conditions = new List<Condition> {
                            new Condition { variable="Health", term="Low" }
                        },
                        actions = new List<ActionDef> {
                            new ActionDef { variable="SpawnRate", term="High" }
                        }
                    },
                    new FuzzyRule
                    {
                        conditions = new List<Condition>
                        {
                            new Condition { variable="Health", term="High"}
                        },
                        actions = new List<ActionDef>
                        {
                            new ActionDef{variable="SpawnRate", term="Low"}
                        }
                    }
                }
            };
            _engine = new FuzzyEngine(config);
        }

        [Test]
        public void LowHealth_Yields_HighSpawnRate()
        {
            var inputs = new Dictionary<string, float> { { "Health", 0.1f } };
            var result = _engine.Infer(inputs);
            Assert.Greater(result["SpawnRate"], 0.8f);
        }

        [Test]
        public void HighHealth_Yields_LowSpawnRate()
        {
            var inputs = new Dictionary<string, float> { { "Health", 1f } };
            var result = _engine.Infer(inputs);
            Assert.Less(result["SpawnRate"], 0.2f);
        }
    }
}
