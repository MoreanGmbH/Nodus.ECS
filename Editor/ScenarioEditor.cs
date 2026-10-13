using System.IO;
using UnityEditor;
using UnityEngine;

namespace Nodus.ECS.Editor {
    [CustomEditor(typeof(ScenarioAsset), true)]
    public sealed class ScenarioEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();

            var asset = (ScenarioAsset)target;
            var children = serializedObject.FindProperty(asset is Scenario ? "Steps" : "Scenarios");
            EditorGUILayout.PropertyField(children, true);

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
            var overrideWait = serializedObject.FindProperty("OverrideWaitFrames");
            EditorGUILayout.PropertyField(overrideWait);
            if (overrideWait.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("WaitFrames"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Screenshots", EditorStyles.boldLabel);
            var overrideScreenshot = serializedObject.FindProperty("OverrideScreenshot");
            EditorGUILayout.PropertyField(overrideScreenshot);
            if (overrideScreenshot.boolValue) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CaptureScreenshot"));
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space(12);

            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying)) {
                if (GUILayout.Button("Run Scenario")) {
                    var runner = Object.FindAnyObjectByType<ScenarioRunner>();
                    if (!runner) runner = new GameObject("Nodus Scenario Runner").AddComponent<ScenarioRunner>();
                    runner.StartCoroutine(runner.Run(asset));
                }
            }
            if (GUILayout.Button("Open Screenshots Folder")) {
                var root = ScenarioRunner.ScreenshotsRoot;
                Directory.CreateDirectory(root);
                EditorUtility.OpenWithDefaultApp(FindLatest(asset, root));
            }
            if (!EditorApplication.isPlaying)
                EditorGUILayout.HelpBox("Enter Play Mode to execute the scenario or container.", MessageType.Info);
        }

        static string FindLatest(ScenarioAsset asset, string root) {
            var prefix = ScenarioRunner.CleanFileName(asset.name) + "_";
            string latest = null;
            foreach (var folder in Directory.GetDirectories(root)) {
                if (!Path.GetFileName(folder).StartsWith(prefix, System.StringComparison.Ordinal)) continue;
                if (latest == null || string.CompareOrdinal(folder, latest) > 0) latest = folder;
            }
            return latest ?? root;
        }
    }
}
