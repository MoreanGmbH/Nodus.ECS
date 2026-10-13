using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nodus.ECS.Definitions {
    [CreateAssetMenu(menuName = "Nodus/ECS/Entities", fileName = "Entities")]
    public sealed class Entities : ScriptableObject {
        [SerializeField] public string Id;
        [SerializeField] public string Store = "Store";
        [SerializeField] public List<Entity> Roots = new();

        public EntitiesData ToData() {
            var result = new EntitiesData { Store = Store };
            foreach (var entity in Roots) {
                if (!entity)
                    throw new InvalidOperationException($"Missing Entity in '{name}'.");
                result.Entities.Add(entity.ToData(new HashSet<Entity>(), Store));
            }
            return result;
        }

        public string ToJson(bool indented = true) => EcsJson.Serialize(ToData(), indented);
    }
}
