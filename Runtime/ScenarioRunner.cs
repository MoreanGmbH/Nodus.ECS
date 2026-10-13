using System;
using System.Collections;
using UnityEngine;

namespace Nodus.ECS {
    // Play in a scene or call Run(Scenario) from a PlayMode test.
    public sealed class ScenarioRunner : MonoBehaviour {
        [SerializeField] public Scenario Scenario;
        [SerializeField] public bool RunOnStart;

        void Start() {
            if (RunOnStart && Scenario) StartCoroutine(Run(Scenario));
        }

        public IEnumerator Run(Scenario scenario) {
            if (!scenario) throw new ArgumentNullException(nameof(scenario));
            foreach (var step in scenario.Steps) {
                if (step.Entities) EcsRuntime.Spawn(step.Entities);
                for (int i = 0; i < step.WaitFrames; i++) yield return null;
                if (!step.CaptureScreenshot) continue;
                yield return new WaitForEndOfFrame();
                string file = string.IsNullOrWhiteSpace(step.ScreenshotName) ? "Scenario" : step.ScreenshotName;
                foreach (char c in System.IO.Path.GetInvalidFileNameChars()) file = file.Replace(c, '_');
                string path = System.IO.Path.Combine(Application.persistentDataPath, file + ".png");
                ScreenCapture.CaptureScreenshot(path);
                Debug.Log($"Nodus.ECS: screenshot requested: {path}");
            }
        }
    }
}
