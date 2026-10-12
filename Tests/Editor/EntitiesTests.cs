using Friflo.Engine.ECS;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Nodus.ECS.Tests {
    [ComponentKey("Nodus.ECS.Tests.TestValue")]
    public struct TestValue : IComponent { public int Value; }

    public class EntitiesTests {
        [OneTimeSetUp]
        public void SetUp() => EcsRegistry.RegisterComponent<TestValue>();

        [Test]
        public void JsonRoundTripAndSpawn() {
            var store = new EntityStore();
            EcsRegistry.RegisterStore("Tests", store);
            try {
                var doc = new EntitiesData { Store = "Tests" };
                doc.Entities.Add(new EntityData { Components = new() {
                    new ComponentData { Key = "Nodus.ECS.Tests.TestValue", Data = JObject.FromObject(new TestValue { Value = 17 }) }
                }});
                var parsed = EcsJson.Parse(EcsJson.Serialize(doc));
                var entity = EcsRuntime.Spawn(parsed)[0];
                Assert.AreEqual(17, entity.GetComponent<TestValue>().Value);
            } finally {
                EcsRegistry.UnregisterStore("Tests", store);
            }
        }

        [Test]
        public void RegistryDoesNotRequireNavigation() {
            Assert.AreEqual("Nodus.ECS.Tests.TestValue", EcsRegistry.GetComponentKey(typeof(TestValue)));
        }

        [Test]
        public void DuplicateComponentsRejected() {
            var doc = new EntitiesData { Store = "Tests" };
            doc.Entities.Add(new EntityData { Components = new() {
                new ComponentData { Key = "Repeat", Data = new JObject() },
                new ComponentData { Key = "Repeat", Data = new JObject() }
            }});
            Assert.Throws<System.InvalidOperationException>(() => EcsJson.Validate(doc));
        }

        [Test]
        public void UrlTokenRoundTrip() {
            var doc = new EntitiesData { Store = "Tests" };
            doc.Entities.Add(new EntityData());
            Assert.AreEqual("Tests", EcsJson.FromUrlToken(EcsJson.ToUrlToken(doc)).Store);
        }

        [Test]
        public void ComponentKeyMatchesFriflo() {
            Assert.AreEqual("Nodus.ECS.Tests.TestValue", EcsRegistry.GetComponentKey(typeof(TestValue)));
        }
    }
}
