using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using EntitiesAsset = Nodus.ECS.Definitions.Entities;
using EntityAsset = Nodus.ECS.Definitions.Entity;

namespace Nodus.ECS.Editor {
    [CustomEditor(typeof(EntitiesAsset))]
    public sealed class EntitiesEditor : UnityEditor.Editor {
        bool showJson;

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Id"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Store"));
            DefinitionInspector.List<EntityAsset>(serializedObject, "Roots", "Entities");

            var definition = (EntitiesAsset)target;
            showJson = EditorGUILayout.Foldout(showJson, "JSON Preview", true);
            if (showJson) {
                try {
                    using (new EditorGUI.DisabledScope(true))
                        EditorGUILayout.TextArea(definition.ToJson(), GUILayout.MinHeight(90));
                } catch (Exception exception) {
                    EditorGUILayout.HelpBox(exception.Message, MessageType.Error);
                }
            }
            if (GUILayout.Button("Copy JSON")) {
                try { EditorGUIUtility.systemCopyBuffer = definition.ToJson(); }
                catch (Exception exception) { Debug.LogException(exception); }
            }
            if (GUILayout.Button("Copy URL Token")) {
                try { EditorGUIUtility.systemCopyBuffer = EcsJson.ToUrlToken(definition.ToData()); }
                catch (Exception exception) { Debug.LogException(exception); }
            }
            if (GUILayout.Button("Validate")) {
                try {
                    var data = definition.ToData();
                    EcsJson.Validate(data);
                    Debug.Log($"Nodus.ECS: '{definition.name}' is valid ({data.Entities.Count} roots, store '{data.Store}').", definition);
                } catch (Exception exception) { Debug.LogException(exception); }
            }
        }
    }

    // Shared Inspector utility for Catalog -> Entities -> Entity -> Component.
    // Kept in this source file so no separate helper script or assembly is required.
    internal static class DefinitionInspector {
        static readonly HashSet<ScriptableObject> Expanded = new();
        static readonly HashSet<ScriptableObject> Drawing = new();
        static readonly Dictionary<ScriptableObject, UnityEditor.Editor> Editors = new();

        internal static void List<T>(SerializedObject owner, string field, string title) where T : ScriptableObject {
            var property = owner.FindProperty(field);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(property, true);
            owner.ApplyModifiedProperties();

            for (int i = 0; i < property.arraySize; i++) {
                var asset = property.GetArrayElementAtIndex(i).objectReferenceValue as T;
                if (asset) Draw(asset);
            }

            if (!GUILayout.Button($"+ New {typeof(T).Name}")) return;
            var created = ScriptableObject.CreateInstance<T>();
            created.name = typeof(T).Name;
            string path = AssetDatabase.GetAssetPath(owner.targetObject);
            if (string.IsNullOrEmpty(path)) {
                UnityEngine.Object.DestroyImmediate(created);
                throw new InvalidOperationException("Save the parent asset before adding embedded assets.");
            }
            AssetDatabase.AddObjectToAsset(created, owner.targetObject);
            Undo.RegisterCreatedObjectUndo(created, "Create ECS definition");
            EditorUtility.SetDirty(owner.targetObject);
            AssetDatabase.SaveAssets();

            owner.Update();
            property = owner.FindProperty(field);
            int index = property.arraySize++;
            property.GetArrayElementAtIndex(index).objectReferenceValue = created;
            owner.ApplyModifiedProperties();
            Expanded.Add(created);
            GUIUtility.ExitGUI();
        }

        static void Draw(ScriptableObject asset) {
            bool open = EditorGUILayout.Foldout(Expanded.Contains(asset), asset.name, true);
            if (open) Expanded.Add(asset); else Expanded.Remove(asset);
            if (!open) return;
            if (!Drawing.Add(asset)) {
                EditorGUILayout.HelpBox("Cyclic asset reference", MessageType.Warning);
                return;
            }
            try {
                EditorGUI.indentLevel++;
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox)) {
                    EditorGUILayout.ObjectField("Asset", asset, asset.GetType(), false);
                    if (!Editors.TryGetValue(asset, out var editor) || !editor) {
                        editor = UnityEditor.Editor.CreateEditor(asset);
                        Editors[asset] = editor;
                    }
                    editor.OnInspectorGUI();
                }
            } finally {
                EditorGUI.indentLevel--;
                Drawing.Remove(asset);
            }
        }
    }
}
