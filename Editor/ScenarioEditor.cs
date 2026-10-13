using UnityEditor;
using UnityEngine;

namespace Nodus.ECS.Editor {
    [CustomEditor(typeof(Scenario))]
    public sealed class ScenarioEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying)) {
                if (GUILayout.Button("Run Scenario")) {
                    var scenario = (Scenario)target;
                    var runner = Object.FindAnyObjectByType<ScenarioRunner>();
                    if (!runner)
                        runner = new GameObject("Nodus Scenario Runner").AddComponent<ScenarioRunner>();
                    runner.StartCoroutine(runner.Run(scenario));
                }
            }
            if (!EditorApplication.isPlaying)
                EditorGUILayout.HelpBox("Enter Play Mode to run the scenario and capture screenshots.", MessageType.Info);
        }
    }
}
