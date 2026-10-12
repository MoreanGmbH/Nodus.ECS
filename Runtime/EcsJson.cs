using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nodus.ECS {
    public static class EcsJson {
        const int MaxLength = 524288;
        const int MaxEntities = 512;
        const int MaxDepth = 32;
        static readonly JsonSerializerSettings Settings = new() {
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            MaxDepth = 64
        };

        public static string Serialize(EntitiesData document, bool indented = false) {
            Validate(document);
            return JsonConvert.SerializeObject(document, indented ? Formatting.Indented : Formatting.None, Settings);
        }

        public static EntitiesData Parse(string json) {
            if (string.IsNullOrWhiteSpace(json) || json.Length > MaxLength)
                throw new ArgumentException("Entities JSON is empty or too large.", nameof(json));
            var document = JsonConvert.DeserializeObject<EntitiesData>(json, Settings);
            Validate(document);
            return document;
        }

        public static void Validate(EntitiesData document) {
            if (document == null || document.Version != 1)
                throw new InvalidOperationException("Unsupported document version.");
            if (document.Entities == null)
                throw new InvalidOperationException("Entities are null.");

            int count = 0;
            foreach (var entity in document.Entities)
                ValidateEntity(entity, document.Store, false, 0, ref count);
        }

        static void ValidateEntity(EntityData entity, string inheritedStore, bool hasParent, int depth, ref int count) {
            if (entity == null || depth > MaxDepth || ++count > MaxEntities)
                throw new InvalidOperationException("Null entity, excessive nesting or too many entities.");

            string store = string.IsNullOrWhiteSpace(entity.Store) ? inheritedStore : entity.Store;
            if (string.IsNullOrWhiteSpace(store))
                throw new InvalidOperationException("Entity has no target store.");
            if (hasParent && store != inheritedStore)
                throw new InvalidOperationException("Parent and child entities must use the same store.");
            if (entity.Components == null)
                throw new InvalidOperationException("Entity components are null.");

            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var component in entity.Components) {
                if (component == null || string.IsNullOrWhiteSpace(component.Key) || !keys.Add(component.Key) || component.Data == null)
                    throw new InvalidOperationException("Invalid, duplicate or missing component data.");
            }
            if (entity.Children == null)
                return;
            foreach (var child in entity.Children)
                ValidateEntity(child, store, true, depth + 1, ref count);
        }

        public static string ToUrlToken(EntitiesData document) {
            var bytes = System.Text.Encoding.UTF8.GetBytes(Serialize(document));
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public static EntitiesData FromUrlToken(string token) {
            if (string.IsNullOrWhiteSpace(token) || token.Length > MaxLength * 2)
                throw new ArgumentException("Invalid URL token.");
            string b64 = token.Replace('-', '+').Replace('_', '/');
            b64 = b64.PadRight((b64.Length + 3) / 4 * 4, '=');
            return Parse(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(b64)));
        }
    }
}
