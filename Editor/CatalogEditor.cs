using System;
using UnityEditor;
using UnityEngine;
using CatalogAsset = Nodus.ECS.Definitions.Catalog;
using EntitiesAsset = Nodus.ECS.Definitions.Entities;

namespace Nodus.ECS.Editor {
    [CustomEditor(typeof(CatalogAsset))]
    public sealed class CatalogEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            DefinitionInspector.List<EntitiesAsset>(serializedObject, "Definitions", "Entities");
            if (!GUILayout.Button("Validate Catalog")) return;
            try {
                var catalog = (CatalogAsset)target;
                catalog.Validate();
                Debug.Log($"Nodus.ECS: Catalog '{catalog.name}' is valid ({catalog.Definitions.Count} definitions).", catalog);
            } catch (Exception exception) { Debug.LogException(exception); }
        }
    }
}
