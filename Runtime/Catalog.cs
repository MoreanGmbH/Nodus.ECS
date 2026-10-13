using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nodus.ECS.Definitions {
    [CreateAssetMenu(menuName = "Nodus/ECS/Catalog", fileName = "Catalog")]
    public sealed class Catalog : ScriptableObject {
        [SerializeField] public List<Entities> Definitions = new();

        public Entities Find(string id) {
            foreach (var definition in Definitions)
                if (definition && definition.Id == id)
                    return definition;
            return null;
        }

        public void Validate() {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var definition in Definitions) {
                if (!definition || string.IsNullOrWhiteSpace(definition.Id) || !ids.Add(definition.Id))
                    throw new InvalidOperationException($"Catalog '{name}' contains a missing/duplicate Entities ID.");
                EcsJson.Validate(definition.ToData());
            }
        }
    }
}
