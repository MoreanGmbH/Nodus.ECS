using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using Newtonsoft.Json.Linq;
using Entities = Nodus.ECS.Definitions.Entities;

namespace Nodus.ECS {
    public static class EcsRuntime {
        public static IReadOnlyList<Entity> Spawn(Entities definition) => Spawn(definition.ToData());
        public static IReadOnlyList<Entity> Spawn(string json) => Spawn(EcsJson.Parse(json));

        public static IReadOnlyList<Entity> Spawn(EntitiesData document) {
            EcsJson.Validate(document);
            var plans = new List<Plan>();
            foreach (var root in document.Entities)
                PlanEntity(root, document.Store, plans);

            var created = new List<Entity>(plans.Count);
            var roots = new List<Entity>(document.Entities.Count);
            try {
                foreach (var plan in plans) {
                    var entity = plan.Store.CreateEntity();
                    created.Add(entity);
                    foreach (var component in plan.Components)
                        component.Reader(entity, component.Data);
                    if (plan.ParentIndex >= 0)
                        created[plan.ParentIndex].AddChild(entity);
                    else
                        roots.Add(entity);
                }
                return roots;
            } catch {
                for (int i = created.Count - 1; i >= 0; i--)
                    if (!created[i].IsNull)
                        created[i].DeleteEntity();
                throw;
            }
        }

        static void PlanEntity(EntityData data, string inheritedStore, List<Plan> plans, int parentIndex = -1) {
            string storeId = string.IsNullOrWhiteSpace(data.Store) ? inheritedStore : data.Store;
            var plan = new Plan { Store = EcsRegistry.GetStore(storeId), ParentIndex = parentIndex };
            foreach (var component in data.Components)
                plan.Components.Add(new ComponentPlan { Reader = EcsRegistry.GetReader(component.Key), Data = component.Data });
            int ownIndex = plans.Count;
            plans.Add(plan);
            if (data.Children == null) return;
            foreach (var child in data.Children)
                PlanEntity(child, storeId, plans, ownIndex);
        }

        sealed class Plan {
            internal EntityStore Store;
            internal int ParentIndex;
            internal List<ComponentPlan> Components = new();
        }

        sealed class ComponentPlan {
            internal Action<Entity, JToken> Reader;
            internal JToken Data;
        }
    }
}
