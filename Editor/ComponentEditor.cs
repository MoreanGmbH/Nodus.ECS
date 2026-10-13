using System;
using System.Linq;
using System.Reflection;
using Friflo.Engine.ECS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using ComponentAsset = Nodus.ECS.Definitions.Component;

namespace Nodus.ECS.Editor {
    [CustomEditor(typeof(ComponentAsset))]
    public sealed class ComponentEditor : UnityEditor.Editor {
        static readonly Type[] Types = TypeCache.GetTypesDerivedFrom<IComponent>()
            .Where(type => type.IsValueType && !type.IsAbstract && !type.ContainsGenericParameters &&
                type.GetCustomAttributesData().Any(attr => attr.AttributeType == typeof(ComponentKeyAttribute)))
            .OrderBy(type => type.FullName).ToArray();
        bool showJson;

        public override void OnInspectorGUI() {
            serializedObject.Update();
            var key = serializedObject.FindProperty("Key");
            var json = serializedObject.FindProperty("Json");
            var names = new string[Types.Length + 1];
            names[0] = "Select component...";
            int selected = 0;
            for (int i = 0; i < Types.Length; i++) {
                names[i + 1] = Types[i].FullName;
                if (EcsRegistry.GetComponentKey(Types[i]) == key.stringValue) selected = i + 1;
            }
            int chosen = EditorGUILayout.Popup("Component", selected, names);
            if (chosen > 0 && chosen != selected) {
                var type = Types[chosen - 1];
                key.stringValue = EcsRegistry.GetComponentKey(type);
                json.stringValue = JsonConvert.SerializeObject(Activator.CreateInstance(type), Formatting.Indented);
            }
            var current = chosen > 0 ? Types[chosen - 1] : null;
            if (current != null) {
                try {
                    var data = JObject.Parse(json.stringValue);
                    bool changed = false;
                    foreach (var field in current.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                        if (field.IsInitOnly) continue;
                        var value = data[field.Name];
                        var type = field.FieldType;
                        if (type == typeof(string)) {
                            var oldValue = value?.ToObject<string>() ?? "";
                            var next = EditorGUILayout.TextField(ObjectNames.NicifyVariableName(field.Name), oldValue);
                            if (next != oldValue) { data[field.Name] = next; changed = true; }
                        } else if (type == typeof(int)) {
                            var oldValue = value?.ToObject<int>() ?? 0;
                            var next = EditorGUILayout.IntField(ObjectNames.NicifyVariableName(field.Name), oldValue);
                            if (next != oldValue) { data[field.Name] = next; changed = true; }
                        } else if (type == typeof(float)) {
                            var oldValue = value?.ToObject<float>() ?? 0f;
                            var next = EditorGUILayout.FloatField(ObjectNames.NicifyVariableName(field.Name), oldValue);
                            if (next != oldValue) { data[field.Name] = next; changed = true; }
                        } else if (type == typeof(bool)) {
                            var oldValue = value?.ToObject<bool>() ?? false;
                            var next = EditorGUILayout.Toggle(ObjectNames.NicifyVariableName(field.Name), oldValue);
                            if (next != oldValue) { data[field.Name] = next; changed = true; }
                        } else if (type.IsEnum) {
                            var oldValue = (Enum)Enum.ToObject(type, value?.ToObject<int>() ?? 0);
                            var next = EditorGUILayout.EnumPopup(ObjectNames.NicifyVariableName(field.Name), oldValue);
                            if (!Equals(next, oldValue)) { data[field.Name] = Convert.ToInt32(next); changed = true; }
                        } else EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(field.Name), "Advanced JSON");
                    }
                    if (changed) json.stringValue = data.ToString(Formatting.Indented);
                } catch (Exception e) { EditorGUILayout.HelpBox(e.Message, MessageType.Error); }
            }
            showJson = EditorGUILayout.Foldout(showJson, "Advanced JSON");
            if (showJson) {
                EditorGUILayout.PropertyField(key);
                EditorGUILayout.PropertyField(json);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
