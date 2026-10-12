using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nodus.ECS {
    [CreateAssetMenu(menuName = "Nodus/ECS/Entities Catalog", fileName = "EntitiesCatalog")]
    public sealed class EntitiesCatalog : ScriptableObject {
        [SerializeField] List<EntitiesDefinition> documents = new();
        public IReadOnlyList<EntitiesDefinition> Definitions => documents;

        public EntitiesDefinition Find(string stableId) {
            foreach (var document in documents)
                if (document && document.StableId == stableId)
                    return document;
            return null;
        }

        public void Validate() {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var document in documents) {
                if (!document || string.IsNullOrWhiteSpace(document.StableId) || !ids.Add(document.StableId))
                    throw new InvalidOperationException("Catalog contains null, empty or duplicate document IDs.");
                EcsJson.Validate(document.ToData());
            }
        }
    }
}
