using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Nodus.ECS.Tests {
    public sealed class NodusSystemSetTests {
        [Test]
        public void SystemOrderSurvivesSerialization() {
            var originalObject = new GameObject("Original");
            var restoredObject = new GameObject("Restored");
            try {
                var original = originalObject.AddComponent<NodusSystemSet>();
                var serialized = new SerializedObject(original);
                var update = serialized.FindProperty("updateSystems");
                update.arraySize = 2;
                update.GetArrayElementAtIndex(0).FindPropertyRelative("typeName").stringValue = "Example.First, Example";
                update.GetArrayElementAtIndex(1).FindPropertyRelative("typeName").stringValue = "Example.Second, Example";
                update.GetArrayElementAtIndex(1).FindPropertyRelative("enabled").boolValue = false;
                serialized.ApplyModifiedProperties();

                var restored = restoredObject.AddComponent<NodusSystemSet>();
                EditorJsonUtility.FromJsonOverwrite(EditorJsonUtility.ToJson(original), restored);
                var result = new SerializedObject(restored).FindProperty("updateSystems");

                Assert.AreEqual(2, result.arraySize);
                Assert.AreEqual("Example.First, Example", result.GetArrayElementAtIndex(0).FindPropertyRelative("typeName").stringValue);
                Assert.AreEqual("Example.Second, Example", result.GetArrayElementAtIndex(1).FindPropertyRelative("typeName").stringValue);
                Assert.IsFalse(result.GetArrayElementAtIndex(1).FindPropertyRelative("enabled").boolValue);
            } finally {
                Object.DestroyImmediate(originalObject);
                Object.DestroyImmediate(restoredObject);
            }
        }
    }
}
