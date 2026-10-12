using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using Newtonsoft.Json.Linq;

namespace Nodus.ECS {
    public static class EcsRegistry {
        static readonly Dictionary<string, Action<Entity, JToken>> Readers = new(StringComparer.Ordinal);
        static readonly Dictionary<string, Type> ComponentTypes = new(StringComparer.Ordinal);
        static readonly Dictionary<string, EntityStore> Stores = new(StringComparer.Ordinal);

        // Allows a known component type to be constructed from JSON using its Friflo component key.
        public static void RegisterComponent<T>() where T : struct, IComponent
            => RegisterComponent<T>(GetComponentKey(typeof(T)));

        public static void RegisterComponent<T>(string key) where T : struct, IComponent {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Component key is required.", nameof(key));
            if (ComponentTypes.TryGetValue(key, out var existing)) {
                if (existing == typeof(T))
                    return;
                throw new InvalidOperationException($"Component key already in use: {key}");
            }
            ComponentTypes.Add(key, typeof(T));
            Readers.Add(key, (entity, data) => entity.AddComponent(data.ToObject<T>()));
        }

        public static string GetComponentKey(Type type) {
            foreach (var attribute in type.GetCustomAttributesData()) {
                if (attribute.AttributeType == typeof(ComponentKeyAttribute))
                    return (string)attribute.ConstructorArguments[0].Value;
            }
            return type.Name;
        }

        // Associates a stable store ID with an existing Friflo EntityStore.
        public static void RegisterStore(string id, EntityStore store) {
            if (string.IsNullOrWhiteSpace(id) || store == null)
                throw new ArgumentException("Store ID and instance are required.");
            if (Stores.TryGetValue(id, out var old) && !ReferenceEquals(old, store))
                throw new InvalidOperationException($"Store ID already in use: {id}");
            Stores[id] = store;
        }

        public static void UnregisterStore(string id, EntityStore store) {
            if (Stores.TryGetValue(id, out var old) && ReferenceEquals(old, store))
                Stores.Remove(id);
        }

        public static EntityStore GetStore(string id) => Stores.TryGetValue(id, out var store)
            ? store : throw new InvalidOperationException($"Store '{id}' is not registered.");

        internal static Action<Entity, JToken> GetReader(string key) => Readers.TryGetValue(key, out var reader)
            ? reader : throw new InvalidOperationException($"Component '{key}' is not registered.");
    }
}
