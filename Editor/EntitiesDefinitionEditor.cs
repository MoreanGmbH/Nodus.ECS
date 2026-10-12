using System;
using UnityEditor;
using UnityEngine;

namespace Nodus.ECS.Editor {
    [CustomEditor(typeof(EntitiesDefinition))]
    public sealed class EntitiesDefinitionEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            var definition = (EntitiesDefinition)target;
            if (GUILayout.Button("Copy JSON"))
                Run(() => EditorGUIUtility.systemCopyBuffer = definition.ToJson());
            if (GUILayout.Button("Copy URL Token"))
                Run(() => EditorGUIUtility.systemCopyBuffer = EcsJson.ToUrlToken(definition.ToData()));
            if (GUILayout.Button("Validate"))
                Run(() => EcsJson.Validate(definition.ToData()));
        }

        static void Run(Action action) {
            try { action(); Debug.Log("Nodus.ECS: completed."); }
            catch (Exception e) { Debug.LogException(e); }
        }
    }
}
