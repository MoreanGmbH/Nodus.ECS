using System;
using Friflo.Engine.ECS;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Nodus.ECS {
    public abstract class ComponentDefinition : ScriptableObject {
        public abstract string Key { get; }
        public abstract JToken GetData();
    }

    public abstract class ComponentDefinition<T> : ComponentDefinition where T : struct, IComponent {
        protected abstract T Value { get; }
        public override JToken GetData() => JToken.FromObject(Value);
    }

    [CreateAssetMenu(menuName = "Nodus/ECS/JSON Component", fileName = "Component")]
    public sealed class JsonComponentDefinition : ComponentDefinition {
        [SerializeField] string componentKey;
        [SerializeField, TextArea(3, 12)] string json = "{}";

        public override string Key => componentKey;
        public override JToken GetData() => JToken.Parse(json);
    }
}
