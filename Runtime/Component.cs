using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Nodus.ECS.Definitions {
    [CreateAssetMenu(menuName = "Nodus/ECS/Component", fileName = "Component")]
    public sealed class Component : ScriptableObject {
        [SerializeField] public string Key;
        [SerializeField, TextArea(3, 12)] public string Json = "{}";

        public JToken GetData() => JToken.Parse(Json);
    }
}
