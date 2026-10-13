using System;
using Catalog = Nodus.ECS.Definitions.Catalog;
using Entities = Nodus.ECS.Definitions.Entities;

namespace Nodus.ECS {
    public static class EcsUrls {
        public static string Reference(string baseUrl, Entities asset) {
            if (!asset || string.IsNullOrWhiteSpace(asset.Id))
                throw new ArgumentException("An Entities asset with a stable ID is required.");
            return Append(baseUrl, "definition", Uri.EscapeDataString(asset.Id));
        }
        public static string Embedded(string baseUrl, EntitiesData document)
            => Append(baseUrl, "data", EcsJson.ToUrlToken(document));

        public static EntitiesData Parse(string url, Catalog catalog = null) {
            var uri = new Uri(url, UriKind.Absolute);
            var query = uri.Query.TrimStart('?').Split('&');
            foreach (var part in query) {
                var index = part.IndexOf('=');
                if (index < 0) continue;
                var key = Uri.UnescapeDataString(part.Substring(0, index));
                var value = Uri.UnescapeDataString(part.Substring(index + 1));
                if (key == "data") return EcsJson.FromUrlToken(value);
                if (key != "definition") continue;
                if (!catalog) throw new InvalidOperationException("A Catalog is required for reference URLs.");
                var definition = catalog.Find(value);
                return definition ? definition.ToData() : throw new InvalidOperationException($"Unknown definition ID '{value}'.");
            }
            throw new ArgumentException("URL has neither a data token nor a definition ID.");
        }
        static string Append(string baseUrl, string key, string value) {
            var uri = new UriBuilder(baseUrl);
            string existing = uri.Query.TrimStart('?');
            uri.Query = (existing.Length == 0 ? "" : existing + "&") + key + "=" + value;
            return uri.Uri.AbsoluteUri;
        }
    }
}
