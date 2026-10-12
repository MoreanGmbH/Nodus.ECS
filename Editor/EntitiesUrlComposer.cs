using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Nodus.ECS.Editor {
    public sealed class EntitiesUrlComposer : EditorWindow {
        [SerializeField] string baseUrl = "https://app.example.com/";
        [SerializeField] List<EntitiesDefinition> definitions = new();

        [MenuItem("Nodus/ECS/URL Composer")]
        static void Open() => GetWindow<EntitiesUrlComposer>("Nodus URL Composer");

        void OnGUI() {
            baseUrl = EditorGUILayout.TextField("Application URL", baseUrl);
            EditorGUILayout.LabelField("Definitions (in order)", EditorStyles.boldLabel);
            for (int i = 0; i < definitions.Count; i++) {
                EditorGUILayout.BeginHorizontal();
                definitions[i] = (EntitiesDefinition)EditorGUILayout.ObjectField(definitions[i], typeof(EntitiesDefinition), false);
                if (GUILayout.Button("×", GUILayout.Width(24)))
                    definitions.RemoveAt(i--);
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("Add Definition"))
                definitions.Add(null);
            if (GUILayout.Button("Copy Embedded JSON URL"))
                CopyEmbedded();
            if (definitions.Count == 1 && definitions[0] && GUILayout.Button("Copy Reference URL")) {
                try {
                    EditorGUIUtility.systemCopyBuffer = EcsUrls.Reference(baseUrl, definitions[0]);
                } catch (Exception exception) {
                    Debug.LogException(exception);
                }
            }
        }

        void CopyEmbedded() {
            try {
                var entities = new EntitiesData();
                foreach (var definition in definitions) {
                    if (!definition)
                        throw new InvalidOperationException("An entities definition reference is empty.");
                    var data = definition.ToData();
                    foreach (var root in data.Entities) {
                        if (string.IsNullOrWhiteSpace(root.Store))
                            root.Store = data.Store;
                        entities.Entities.Add(root);
                    }
                }
                EditorGUIUtility.systemCopyBuffer = EcsUrls.Embedded(baseUrl, entities);
                Debug.Log("Nodus.ECS: JSON URL copied.");
            } catch (Exception exception) {
                Debug.LogException(exception);
            }
        }
    }
}
