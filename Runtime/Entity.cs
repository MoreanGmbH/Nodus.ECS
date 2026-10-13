using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nodus.ECS.Definitions {
    [CreateAssetMenu(menuName = "Nodus/ECS/Entity", fileName = "Entity")]
    public sealed class Entity : ScriptableObject {
        [SerializeField] public string Id;
        [SerializeField] public string Store;
        [SerializeField] public List<Component> Components = new();
        [SerializeField] public List<Entity> Children = new();

        public EntityData ToData(HashSet<Entity> ancestors, string inheritedStore, bool hasParent = false) {
            if (!ancestors.Add(this))
                throw new InvalidOperationException($"Cyclic Entity reference: {name}");
            try {
                string targetStore = string.IsNullOrWhiteSpace(Store) ? inheritedStore : Store;
                if (hasParent && targetStore != inheritedStore)
                    throw new InvalidOperationException($"Child '{name}' must use parent store '{inheritedStore}'.");

                var data = new EntityData {
                    Id = string.IsNullOrWhiteSpace(Id) ? null : Id,
                    Store = string.IsNullOrWhiteSpace(Store) ? null : Store
                };
                var keys = new HashSet<string>(StringComparer.Ordinal);
                foreach (var component in Components) {
                    if (!component)
                        throw new InvalidOperationException($"Missing Component on Entity '{name}'.");
                    if (string.IsNullOrWhiteSpace(component.Key) || !keys.Add(component.Key))
                        throw new InvalidOperationException($"Missing/duplicate component key '{component.Key}' on Entity '{name}'.");
                    data.Components.Add(new ComponentData { Key = component.Key, Data = component.GetData() });
                }
                if (Children.Count > 0) {
                    data.Children = new List<EntityData>(Children.Count);
                    foreach (var child in Children) {
                        if (!child)
                            throw new InvalidOperationException($"Missing child Entity on '{name}'.");
                        data.Children.Add(child.ToData(ancestors, targetStore, true));
                    }
                }
                return data;
            } finally {
                ancestors.Remove(this);
            }
        }
    }
}
