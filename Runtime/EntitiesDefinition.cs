using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nodus.ECS {
    [CreateAssetMenu(menuName = "Nodus/ECS/Entities Definition", fileName = "Entities")]
    public sealed class EntitiesDefinition : ScriptableObject {
        [SerializeField] string stableId;
        [SerializeField] string storeId = "Store";
        [SerializeField] List<EntityDefinition> entities = new();

        public string StableId => stableId;
        public string StoreId => storeId;
        public IReadOnlyList<EntityDefinition> Entities => entities;

        public EntitiesData ToData() {
            var result = new EntitiesData { Store = storeId };
            foreach (var definition in entities) {
                if (!definition)
                    throw new InvalidOperationException($"Null entity in {name}.");
                result.Entities.Add(definition.ToData(new HashSet<EntityDefinition>(), storeId));
            }
            return result;
        }

        public string ToJson(bool indented = true) => EcsJson.Serialize(ToData(), indented);
    }
}
