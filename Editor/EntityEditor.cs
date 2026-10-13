using UnityEditor;
using EntityAsset = Nodus.ECS.Definitions.Entity;
using ComponentAsset = Nodus.ECS.Definitions.Component;

namespace Nodus.ECS.Editor {
    [CustomEditor(typeof(EntityAsset))]
    public sealed class EntityEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Id"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Store"));
            DefinitionInspector.List<ComponentAsset>(serializedObject, "Components", "Components");
            DefinitionInspector.List<EntityAsset>(serializedObject, "Children", "Children");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
