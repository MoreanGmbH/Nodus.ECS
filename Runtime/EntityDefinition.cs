using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nodus.ECS {
    [CreateAssetMenu(menuName = "Nodus/ECS/Entity Definition", fileName = "Entity")]
    public sealed class EntityDefinition : ScriptableObject {
        [SerializeField] string stableId;
        [SerializeField] string storeId;
        [SerializeField] List<ComponentDefinition> components = new();
        [SerializeField] List<EntityDefinition> children = new();

        public string StableId => stableId;
        public string StoreId => storeId;
        public IReadOnlyList<ComponentDefinition> Components => components;
        public IReadOnlyList<EntityDefinition> Children => children;

        internal EntityData ToData(HashSet<EntityDefinition> ancestors, string inheritedStore, bool hasParent = false) {
            if (!ancestors.Add(this))
                throw new InvalidOperationException($"Cyclic EntityDefinition reference: {name}");
            try {
                string target = string.IsNullOrWhiteSpace(storeId) ? inheritedStore : storeId;
                if (hasParent && !string.IsNullOrEmpty(inheritedStore) && target != inheritedStore)
                    throw new InvalidOperationException($"Child '{name}' cannot use store '{target}' beneath parent in '{inheritedStore}'.");

                var data = new EntityData { Id = string.IsNullOrWhiteSpace(stableId) ? null : stableId, Store = string.IsNullOrWhiteSpace(storeId) ? null : storeId };
                var seen = new HashSet<string>(StringComparer.Ordinal);
                foreach (var component in components) {
                    if (!component)
                        throw new InvalidOperationException($"Null component definition on {name}.");
                    if (string.IsNullOrWhiteSpace(component.Key) || !seen.Add(component.Key))
                        throw new InvalidOperationException($"Missing/duplicate component key '{component.Key}' on {name}.");
                    data.Components.Add(new ComponentData { Key = component.Key, Data = component.GetData() });
                }
                if (children.Count > 0) {
                    data.Children = new List<EntityData>(children.Count);
                    foreach (var child in children) {
                        if (!child)
                            throw new InvalidOperationException($"Null child definition on {name}.");
                        data.Children.Add(child.ToData(ancestors, target, true));
                    }
                }
                return data;
            } finally {
                ancestors.Remove(this);
            }
        }
    }
}
