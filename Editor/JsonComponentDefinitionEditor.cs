using System;
using System.Linq;
using System.Reflection;
using Friflo.Engine.ECS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Nodus.ECS.Editor {
    [CustomEditor(typeof(JsonComponentDefinition))]
    public sealed class JsonComponentDefinitionEditor : UnityEditor.Editor {
        static readonly Type[] Types = TypeCache.GetTypesDerivedFrom<IComponent>()
            .Where(type => type.IsValueType && !type.IsAbstract && !type.ContainsGenericParameters &&
                type.GetCustomAttributesData().Any(attr => attr.AttributeType == typeof(ComponentKeyAttribute)))
            .OrderBy(type => type.FullName)
            .ToArray();
        bool showJson;

        public override void OnInspectorGUI() {
            serializedObject.Update();
            var key = serializedObject.FindProperty("componentKey");
            var json = serializedObject.FindProperty("json");

            var names = new string[Types.Length + 1];
            names[0] = "Select component...";
            int selected = 0;
            for (int i = 0; i < Types.Length; i++) {
                names[i + 1] = Types[i].FullName;
                if (EcsRegistry.GetComponentKey(Types[i]) == key.stringValue)
                    selected = i + 1;
            }

            int chosen = EditorGUILayout.Popup("Component", selected, names);
            if (chosen > 0 && chosen != selected) {
                var type = Types[chosen - 1];
                key.stringValue = EcsRegistry.GetComponentKey(type);
                json.stringValue = JsonConvert.SerializeObject(Activator.CreateInstance(type), Formatting.Indented);
            }

            if (chosen == 0 && selected == 0)
                EditorGUILayout.HelpBox("Select a component with a [ComponentKey] attribute.", MessageType.Info);

            var current = chosen > 0 ? Types[chosen - 1] : null;
            if (current != null) {
                try {
                    var data = JObject.Parse(json.stringValue);
                    bool changed = false;
                    foreach (var field in current.GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                        if (field.IsInitOnly)
                            continue;
                        var fieldType = field.FieldType;
                        var value = data[field.Name];
                        if (fieldType == typeof(string)) {
                            string oldValue = value?.ToObject<string>() ?? "";
                            string newValue = EditorGUILayout.TextField(ObjectNames.NicifyVariableName(field.Name), oldValue);
                            if (newValue != oldValue) { data[field.Name] = newValue; changed = true; }
                        } else if (fieldType == typeof(int)) {
                            int oldValue = value?.ToObject<int>() ?? 0;
                            int newValue = EditorGUILayout.IntField(ObjectNames.NicifyVariableName(field.Name), oldValue);
                            if (newValue != oldValue) { data[field.Name] = newValue; changed = true; }
                        } else if (fieldType == typeof(float)) {
                            float oldValue = value?.ToObject<float>() ?? 0;
                            float newValue = EditorGUILayout.FloatField(ObjectNames.NicifyVariableName(field.Name), oldValue);
                            if (newValue != oldValue) { data[field.Name] = newValue; changed = true; }
                        } else if (fieldType == typeof(bool)) {
                            bool oldValue = value?.ToObject<bool>() ?? false;
                            bool newValue = EditorGUILayout.Toggle(ObjectNames.NicifyVariableName(field.Name), oldValue);
                            if (newValue != oldValue) { data[field.Name] = newValue; changed = true; }
                        } else if (fieldType.IsEnum) {
                            var oldValue = value == null ? (Enum)Enum.GetValues(fieldType).GetValue(0) : (Enum)Enum.ToObject(fieldType, value.ToObject<int>());
                            var newValue = EditorGUILayout.EnumPopup(ObjectNames.NicifyVariableName(field.Name), oldValue);
                            if (!Equals(newValue, oldValue)) { data[field.Name] = Convert.ToInt32(newValue); changed = true; }
                        } else {
                            EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(field.Name), "Edit in Advanced JSON");
                        }
                    }
                    if (changed)
                        json.stringValue = data.ToString(Formatting.Indented);
                } catch (Exception exception) {
                    EditorGUILayout.HelpBox($"Invalid component JSON: {exception.Message}", MessageType.Error);
                }
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
