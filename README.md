# Nodus.ECS

Reusable Unity assets that describe Friflo ECS components and entities. The same definitions can generate JSON, instantiate entities, and be referenced from other assets.

## Installation

```powershell
git submodule add https://github.com/MoreanGmbH/Nodus.ECS.git Packages/com.nodus.ecs
```

Requires Unity 6.6, Friflo.Engine.ECS and `com.unity.nuget.newtonsoft-json`.

## Designer

1. **Component** — `Assets > Create > Nodus > ECS > JSON Component`. Select a component type and enter its fields.
2. **Entity** — `Assets > Create > Nodus > ECS > Entity Definition`. Drag Component assets into **Components**; optionally add child Entity assets.
3. **Entities** — `Assets > Create > Nodus > ECS > Entities Definition`. Add Entity assets, set **Store Id** (for example `UI`) and a stable ID.
4. **Use** — Assign the Entities asset to an `EntitiesButton` in UI Builder or a serialized asset reference.
5. **Export** — Use **Copy JSON** on the Entities asset or `Nodus > ECS > URL Composer` for an embedded URL.

Create asset instances under `Assets/<Project>/Definitions/`.

## Developer

Register components in the MonoBehaviour that owns each package's ECSStore:

```csharp
void Awake() {
    Store = GetComponent<ECSStore>().EntityStore;
    EcsRegistry.RegisterStore("MyPackage", Store);
    EcsRegistry.RegisterComponent<MyComponent>();
}
```

`RegisterComponent<T>()` uses `[ComponentKey]` and accepts only explicitly registered types when loading JSON. Registration is global; stores are registered by logical ID. An `EcsStoreBinding` may be used where no package startup script is available.

```csharp
EcsRuntime.Spawn(entitiesAsset);
EcsRuntime.Spawn(json);
EcsRuntime.Spawn(EcsUrls.Parse(url));
```

The last call accepts embedded `?data=` URLs. Reference `?definition=` URLs require the optional `EntitiesCatalog` for stable-ID resolution.

## JSON

```json
{
  "Version": 1,
  "Store": "UI",
  "Entities": [
    {
      "Components": [
        { "Key": "Nodus.UI.PushView", "Data": { "StackId": "Panels", "ViewId": "Feedback" } }
      ]
    }
  ]
}
```

Entity definitions support multiple components, ordered children and optional per-root store overrides. `CommandStatus` indicates that a command is awaiting processing; the consuming ECS system marks it handled.

## Tests

Run EditMode tests. If Unity doesn't discover them, add `"testables": ["com.nodus.ecs"]` to `Packages/manifest.json`.

## System configuration

Add `ECSStore` and `NodusSystemSet` to the same GameObject. In the **Nodus System Set** Inspector, click **+** to choose systems, drag to reorder them, and toggle individual systems on or off. Use the **Start**, **Update**, **FixedUpdate**, and **LateUpdate** lists for their respective Unity lifecycle phases. The configured lists are serialized on the component, including prefab variants.

To migrate, temporarily add `NodusSystemSet` next to an existing `ECSSystemSet` and click **Import from ECSSystemSet**. Once imported, **remove `ECSSystemSet`**; both runners must not execute together. Import supports the four standard lifecycle groups with flat system lists.

System types need a public parameterless constructor. Missing/renamed systems are highlighted in the Inspector and reported as errors at runtime. No Odin dependency is needed.
