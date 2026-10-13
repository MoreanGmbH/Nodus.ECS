# Nodus.ECS

Unity 6.6 + Friflo ECS entity authoring and JSON execution.

## Installation

```powershell
git submodule add https://github.com/MoreanGmbH/Nodus.ECS.git Packages/com.nodus.ecs
```

## Assets

- **Component** (`Nodus.ECS.Definitions.Component`): `Key`, `Json` (structured in the Inspector).
- **Entity** (`Nodus.ECS.Definitions.Entity`): `Id`, `Store`, `Components`, `Children`.
- **Entities** (`Nodus.ECS.Definitions.Entities`): `Id`, `Store`, `Roots`.
- **Catalog** (`Nodus.ECS.Definitions.Catalog`): `Definitions`, an optional ID lookup for URLs.

All fields are public and serializable. Names intentionally changed during pre-alpha.
Edit referenced or embedded assets inline in parent Inspectors. Shared Component asset edits affect every Entity that references it.

## Execution

```csharp
EcsRuntime.Spawn(entitiesAsset);
EcsRuntime.Spawn(entitiesAsset.ToJson());
EcsRuntime.Spawn(EcsUrls.Parse(url, optionalCatalog));
```

`EntitiesButton` accepts an `Entities` asset directly. `?definition=<Id>` requires a Catalog; `?data=<base64url>` embeds the JSON contract.

## Scenarios

Scenario assets contain ordered references to `Entities` assets. `ScenarioRunner` can play a scenario and capture screenshots in Play Mode.

## Setup

Register an ECS store using `EcsRegistry.RegisterStore` or your package's runtime bootstrap. Component types must also be registered. For public URLs, use Catalog-based IDs instead of allowing arbitrary component commands.
