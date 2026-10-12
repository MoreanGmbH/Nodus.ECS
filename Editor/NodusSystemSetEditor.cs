using System;
using System.Collections.Generic;
using System.Linq;
using Friflo.Engine.ECS.Systems;
using Friflo.Engine.Unity;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;

namespace Nodus.ECS.Editor {
    [CustomEditor(typeof(NodusSystemSet))]
    public sealed class NodusSystemSetEditor : UnityEditor.Editor {
        static readonly string[] Fields = {
            "startSystems", "updateSystems", "fixedUpdateSystems", "lateUpdateSystems"
        };

        static readonly string[] Titles = {
            "Start", "Update", "FixedUpdate", "LateUpdate"
        };

        static readonly SystemPhase[] Phases = {
            SystemPhase.Start, SystemPhase.Update, SystemPhase.FixedUpdate, SystemPhase.LateUpdate
        };

        ReorderableList[] lists;
        SerializedProperty monitorPerformance;

        static string StableName(Type type) => $"{type.FullName}, {type.Assembly.GetName().Name}";

        static Type[] DiscoverSystems() {
            var playerAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Player)
                .Select(assembly => assembly.name).ToHashSet();

            return TypeCache.GetTypesDerivedFrom<BaseSystem>()
                .Where(type => !type.IsAbstract && !type.ContainsGenericParameters &&
                               !typeof(SystemGroup).IsAssignableFrom(type) &&
                               type.GetConstructor(Type.EmptyTypes) != null &&
                               playerAssemblies.Contains(type.Assembly.GetName().Name))
                .OrderBy(type => type.FullName)
                .ToArray();
        }

        static bool SupportsPhase(Type type, int phaseIndex) {
            var phase = (SystemPhaseAttribute)Attribute.GetCustomAttribute(type, typeof(SystemPhaseAttribute));
            return phase == null || (phase.Phases & Phases[phaseIndex]) != 0;
        }

        void OnEnable() {
            monitorPerformance = serializedObject.FindProperty("monitorPerformance");
            lists = new ReorderableList[Fields.Length];
            for (var i = 0; i < Fields.Length; i++) {
                var index = i;
                var property = serializedObject.FindProperty(Fields[i]);
                var list = new ReorderableList(serializedObject, property, true, true, true, true);
                list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, $"{Titles[index]} ({property.arraySize})");
                list.drawElementCallback = (rect, elementIndex, active, focused) =>
                    DrawElement(rect, property.GetArrayElementAtIndex(elementIndex));
                list.elementHeight = EditorGUIUtility.singleLineHeight + 6;
                list.onAddDropdownCallback = (rect, targetList) => ShowAddMenu(index, property);
                lists[i] = list;
            }
        }

        static void DrawElement(Rect rect, SerializedProperty element) {
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;
            var enabled = element.FindPropertyRelative("enabled");
            var typeName = element.FindPropertyRelative("typeName");

            enabled.boolValue = EditorGUI.Toggle(new Rect(rect.x, rect.y, 18, rect.height), enabled.boolValue);
            var type = Type.GetType(typeName.stringValue, throwOnError: false);
            var label = type != null ? type.FullName : $"MISSING: {typeName.stringValue}";
            var style = type != null ? EditorStyles.label : EditorStyles.boldLabel;
            var oldColor = GUI.color;
            if (type == null)
                GUI.color = Color.red;
            EditorGUI.LabelField(new Rect(rect.x + 22, rect.y, rect.width - 22, rect.height),
                new GUIContent(label, typeName.stringValue), style);
            GUI.color = oldColor;
        }

        HashSet<string> AssignedNames() {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var field in Fields) {
                var list = serializedObject.FindProperty(field);
                for (var i = 0; i < list.arraySize; i++) {
                    var storedName = list.GetArrayElementAtIndex(i).FindPropertyRelative("typeName").stringValue;
                    var type = Type.GetType(storedName, throwOnError: false);
                    names.Add(type != null ? StableName(type) : storedName);
                }
            }
            return names;
        }

        void ShowAddMenu(int phaseIndex, SerializedProperty list) {
            serializedObject.Update();
            var assigned = AssignedNames();
            var candidates = DiscoverSystems()
                .Where(type => SupportsPhase(type, phaseIndex) && !assigned.Contains(StableName(type)))
                .ToArray();
            var menu = new GenericMenu();
            foreach (var type in candidates) {
                var stableName = StableName(type);
                menu.AddItem(new GUIContent($"{type.Assembly.GetName().Name}/{type.FullName}"), false, () => {
                    serializedObject.Update();
                    if (AssignedNames().Contains(stableName))
                        return;
                    list.arraySize++;
                    var element = list.GetArrayElementAtIndex(list.arraySize - 1);
                    element.FindPropertyRelative("typeName").stringValue = stableName;
                    element.FindPropertyRelative("enabled").boolValue = true;
                    serializedObject.ApplyModifiedProperties();
                });
            }
            if (candidates.Length == 0)
                menu.AddDisabledItem(new GUIContent("No available systems"));
            menu.ShowAsContext();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            using (new EditorGUI.DisabledScope(Application.isPlaying)) {
                EditorGUILayout.PropertyField(monitorPerformance);
                foreach (var list in lists) {
                    EditorGUILayout.Space(5);
                    list.DoLayoutList();
                }
            }
            serializedObject.ApplyModifiedProperties();

            var existing = ((NodusSystemSet)target).GetComponent<ECSSystemSet>();
            if (existing != null) {
                EditorGUILayout.HelpBox("ECSSystemSet is still attached. Import its systems, then remove it before Play Mode.", MessageType.Warning);
                if (GUILayout.Button("Import from ECSSystemSet"))
                    Import(existing);
            }

            if (Application.isPlaying && ((NodusSystemSet)target).Root is { } root) {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Stores: {root.Stores.Count}");
                if (GUILayout.Button("Log System Performance"))
                    Debug.Log(root.GetPerfLog());
            }
        }

        void Import(ECSSystemSet oldSet) {
            var oldSerialized = new SerializedObject(oldSet);
            oldSerialized.Update();
            var source = oldSerialized.FindProperty("ecsSystems");
            if (source == null)
                throw new InvalidOperationException("Cannot find ECSSystemSet.ecsSystems.");

            var imported = new List<(string name, bool enabled)>[Fields.Length];
            for (var i = 0; i < imported.Length; i++)
                imported[i] = new List<(string, bool)>();

            for (var i = 0; i < source.arraySize; i++)
                ReadLegacy(source.GetArrayElementAtIndex(i), -1, imported);

            if (imported.Sum(group => group.Count) == 0) {
                EditorUtility.DisplayDialog("Import ECS Systems", "No supported saved systems were found.", "OK");
                return;
            }

            serializedObject.Update();
            for (var i = 0; i < Fields.Length; i++) {
                var list = serializedObject.FindProperty(Fields[i]);
                list.ClearArray();
                foreach (var item in imported[i]) {
                    list.arraySize++;
                    var element = list.GetArrayElementAtIndex(list.arraySize - 1);
                    element.FindPropertyRelative("typeName").stringValue = item.name;
                    element.FindPropertyRelative("enabled").boolValue = item.enabled;
                }
            }
            serializedObject.ApplyModifiedProperties();
            EditorUtility.DisplayDialog("Import ECS Systems", "Systems imported. Remove ECSSystemSet before entering Play Mode.", "OK");
        }

        static void ReadLegacy(SerializedProperty item, int phase, List<(string name, bool enabled)>[] imported) {
            var typeName = item.FindPropertyRelative("type").stringValue;
            var value = item.FindPropertyRelative("value").stringValue;
            var children = item.FindPropertyRelative("systems");
            var data = !string.IsNullOrEmpty(value) ? JObject.Parse(value) : new JObject();
            var enabled = data.Value<bool?>("enabled") ?? data.Value<bool?>("Enabled") ?? true;

            if (typeName == typeof(SystemGroup).FullName) {
                var groupName = data.Value<string>("name") ?? data.Value<string>("Name");
                var phaseIndex = Array.IndexOf(Titles, groupName);
                if (phaseIndex < 0)
                    throw new InvalidOperationException($"Cannot import custom nested group '{groupName}'.");
                for (var i = 0; i < children.arraySize; i++)
                    ReadLegacy(children.GetArrayElementAtIndex(i), phaseIndex, imported);
                return;
            }

            if (children != null && children.arraySize > 0)
                throw new InvalidOperationException($"Cannot import nested systems from '{typeName}'.");
            if (phase < 0)
                throw new InvalidOperationException($"System '{typeName}' is outside a lifecycle group.");

            var systems = DiscoverSystems();
            var type = systems.FirstOrDefault(system => system.FullName == typeName);
            if (type == null)
                throw new InvalidOperationException($"Cannot resolve system '{typeName}' from ECSSystemSet.");
            imported[phase].Add((StableName(type), enabled));
        }
    }
}
