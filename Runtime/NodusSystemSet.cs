using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Unity;
using UnityEngine;

namespace Nodus.ECS {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ECSStore))]
    [DefaultExecutionOrder(500)]
    public sealed class NodusSystemSet : MonoBehaviour {
        [Serializable]
        public sealed class SystemEntry {
            [SerializeField] string typeName;
            [SerializeField] bool enabled = true;

            public string TypeName => typeName;
            public bool Enabled => enabled;
        }

        [SerializeField, HideInInspector] List<SystemEntry> startSystems = new();
        [SerializeField, HideInInspector] List<SystemEntry> updateSystems = new();
        [SerializeField, HideInInspector] List<SystemEntry> fixedUpdateSystems = new();
        [SerializeField, HideInInspector] List<SystemEntry> lateUpdateSystems = new();
        [SerializeField] bool monitorPerformance;

        SystemRoot root;
        SystemGroup startGroup;
        SystemGroup updateGroup;
        SystemGroup fixedUpdateGroup;
        SystemGroup lateUpdateGroup;
        EntityStore store;

        public SystemRoot Root => root;

        void Awake() {
            if (GetComponent<ECSSystemSet>() != null)
                throw new InvalidOperationException("Remove ECSSystemSet before using NodusSystemSet.");

            store = GetComponent<ECSStore>().EntityStore;
            root = new SystemRoot(store);
            startGroup = new SystemGroup("Start");
            updateGroup = new SystemGroup("Update");
            fixedUpdateGroup = new SystemGroup("FixedUpdate");
            lateUpdateGroup = new SystemGroup("LateUpdate");

            root.Add(startGroup);
            root.Add(updateGroup);
            root.Add(fixedUpdateGroup);
            root.Add(lateUpdateGroup);

            var addedTypes = new HashSet<Type>();
            AddSystems(startGroup, startSystems, SystemPhase.Start, addedTypes);
            AddSystems(updateGroup, updateSystems, SystemPhase.Update, addedTypes);
            AddSystems(fixedUpdateGroup, fixedUpdateSystems, SystemPhase.FixedUpdate, addedTypes);
            AddSystems(lateUpdateGroup, lateUpdateSystems, SystemPhase.LateUpdate, addedTypes);
            root.SetMonitorPerf(monitorPerformance);
        }

        void Start() => startGroup.Update(new UpdateTick(Time.deltaTime, Time.time));
        void Update() => updateGroup.Update(new UpdateTick(Time.deltaTime, Time.time));
        void FixedUpdate() => fixedUpdateGroup.Update(new UpdateTick(Time.fixedDeltaTime, Time.fixedTime));
        void LateUpdate() => lateUpdateGroup.Update(new UpdateTick(Time.deltaTime, Time.time));

        void OnDestroy() {
            if (root != null)
                root.RemoveStore(store);
        }

        static void AddSystems(SystemGroup group, List<SystemEntry> entries, SystemPhase phase, HashSet<Type> addedTypes) {
            foreach (var entry in entries) {
                var type = Type.GetType(entry.TypeName, throwOnError: false);
                if (type == null || type.IsAbstract || !typeof(BaseSystem).IsAssignableFrom(type) ||
                    typeof(SystemGroup).IsAssignableFrom(type) || type.GetConstructor(Type.EmptyTypes) == null)
                    throw new InvalidOperationException($"Invalid ECS system '{entry.TypeName}' in group '{group.Name}'.");

                var attribute = (SystemPhaseAttribute)Attribute.GetCustomAttribute(type, typeof(SystemPhaseAttribute));
                if (attribute != null && (attribute.Phases & phase) == 0)
                    throw new InvalidOperationException($"System '{type.FullName}' cannot run in '{group.Name}'.");
                if (!addedTypes.Add(type))
                    throw new InvalidOperationException($"System '{type.FullName}' is assigned more than once.");

                var system = (BaseSystem)Activator.CreateInstance(type);
                system.Enabled = entry.Enabled;
                group.Add(system);
            }
        }
    }
}
