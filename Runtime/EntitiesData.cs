using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Nodus.ECS {
    public sealed class EntitiesData {
        [JsonProperty("Version")] public int Version = 1;
        [JsonProperty("Store", NullValueHandling = NullValueHandling.Ignore)] public string Store;
        [JsonProperty("Entities")] public List<EntityData> Entities = new();
    }

    public sealed class EntityData {
        [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)] public string Id;
        [JsonProperty("Store", NullValueHandling = NullValueHandling.Ignore)] public string Store;
        [JsonProperty("Components")] public List<ComponentData> Components = new();
        [JsonProperty("Children", NullValueHandling = NullValueHandling.Ignore)] public List<EntityData> Children;
    }

    public sealed class ComponentData {
        [JsonProperty("Key")] public string Key;
        [JsonProperty("Data")] public JToken Data;
    }
}
