using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using Entities = Nodus.ECS.Definitions.Entities;

namespace Nodus.ECS {
    public struct ScenarioExecutionStep {
        public Entities Entities;
        public int WaitFrames;
        public bool CaptureScreenshot;
        public string ScreenshotName;
    }

    public sealed class ScenarioRunner : MonoBehaviour {
        [Tooltip("Scenario or Scenario Container to run automatically or via code.")]
        [SerializeField] public ScenarioAsset Scenario;

        [Tooltip("Run the assigned scenario when this component starts in Play Mode.")]
        [SerializeField] public bool RunOnStart;

        // persistentDataPath already includes the application product name (often Nodus).
        public static string ScreenshotsRoot => Path.Combine(Application.persistentDataPath, "Scenarios");
        public static string LastScreenshotFolder { get; private set; }

        // Scenarios must give ECS and UI systems time to process each action.
        public const int DefaultWaitFrames = 60;
        public const bool DefaultCaptureScreenshot = true;

        void Start() {
            if (RunOnStart && Scenario)
                StartCoroutine(Run(Scenario));
        }

        public static IReadOnlyList<ScenarioExecutionStep> Plan(ScenarioAsset root) {
            if (!root) throw new ArgumentNullException(nameof(root));
            var result = new List<ScenarioExecutionStep>();
            Expand(root, null, null, new HashSet<ScenarioAsset>(), result);
            return result;
        }

        static void Expand(ScenarioAsset asset, int? frames, bool? screenshot,
            HashSet<ScenarioAsset> ancestors, List<ScenarioExecutionStep> output) {
            if (!asset) throw new InvalidOperationException("Missing scenario or container reference.");
            if (!ancestors.Add(asset)) throw new InvalidOperationException($"Cyclic scenario reference: {asset.name}");

            try {
                // Once a parent specifies an override, descendants cannot replace it.
                if (!frames.HasValue && asset.OverrideWaitFrames)
                    frames = Mathf.Max(0, asset.WaitFrames);
                if (!screenshot.HasValue && asset.OverrideScreenshot)
                    screenshot = asset.CaptureScreenshot;

                if (asset is Scenario scenario) {
                    foreach (var step in scenario.Steps) {
                        if (!step.Entities)
                            throw new InvalidOperationException($"Missing Entities reference in Scenario '{scenario.name}'.");
                        output.Add(new ScenarioExecutionStep {
                            Entities = step.Entities,
                            WaitFrames = Mathf.Max(1, frames ?? DefaultWaitFrames),
                            CaptureScreenshot = screenshot ?? DefaultCaptureScreenshot,
                            ScreenshotName = step.Entities.name
                        });
                    }
                } else if (asset is ScenarioContainer container) {
                    foreach (var child in container.Scenarios)
                        Expand(child, frames, screenshot, ancestors, output);
                } else {
                    throw new InvalidOperationException($"Unsupported scenario asset: {asset.name}");
                }
            } finally {
                ancestors.Remove(asset);
            }
        }

        public IEnumerator Run(ScenarioAsset scenario) {
            var steps = Plan(scenario);
            string folder = null;
            LastScreenshotFolder = null;

            for (int i = 0; i < steps.Count; i++) {
                var step = steps[i];
                if (step.Entities) EcsRuntime.Spawn(step.Entities);
                for (int frame = 0; frame < step.WaitFrames; frame++) yield return null;
                if (!step.CaptureScreenshot) continue;

                yield return new WaitForEndOfFrame();
                if (folder == null) {
                    var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture);
                    var prefix = CleanFileName(scenario.name) + "_" + stamp;
                    folder = Path.Combine(ScreenshotsRoot, prefix);
                    for (int suffix = 1; Directory.Exists(folder); suffix++)
                        folder = Path.Combine(ScreenshotsRoot, prefix + "_" + suffix);
                    Directory.CreateDirectory(folder);
                    LastScreenshotFolder = folder;
                }

                var label = string.IsNullOrWhiteSpace(step.ScreenshotName)
                    ? (step.Entities ? step.Entities.name : "Step") : step.ScreenshotName;
                var path = Path.Combine(folder, ScreenshotFileName(i, label));
                ScreenCapture.CaptureScreenshot(path);
                Debug.Log($"Nodus.ECS: screenshot requested: {path}");
                yield return null;
            }
        }

        // The first element is 1, not 0. Numbering is unique across nested scenarios.
        public static string ScreenshotFileName(int stepIndex, string label)
            => $"{stepIndex + 1:D3}_{CleanFileName(label)}.png";

        public static string CleanFileName(string value) {
            if (string.IsNullOrWhiteSpace(value)) return "Scenario";
            foreach (var c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
            return value.TrimEnd('.', ' ');
        }
    }
}
