using System;
using NUnit.Framework;
using UnityEngine;
using Entities = Nodus.ECS.Definitions.Entities;

namespace Nodus.ECS.Tests {
    public sealed class ScenarioTests {
        [Test]
        public void OutermostOverridesWin() {
            var root = ScriptableObject.CreateInstance<ScenarioContainer>();
            var middle = ScriptableObject.CreateInstance<ScenarioContainer>();
            var inner = ScriptableObject.CreateInstance<Scenario>();
            var action = ScriptableObject.CreateInstance<Entities>();
            try {
                root.OverrideWaitFrames = true;
                root.WaitFrames = 10;
                root.OverrideScreenshot = true;
                root.CaptureScreenshot = false;
                middle.OverrideWaitFrames = true;
                middle.WaitFrames = 8;
                inner.OverrideScreenshot = true;
                inner.CaptureScreenshot = true;
                inner.Steps.Add(new ScenarioStep { Entities = action });
                middle.Scenarios.Add(inner);
                root.Scenarios.Add(middle);
                var steps = ScenarioRunner.Plan(root);
                Assert.AreEqual(1, steps.Count);
                Assert.AreEqual(10, steps[0].WaitFrames);
                Assert.IsFalse(steps[0].CaptureScreenshot);
            } finally {
                UnityEngine.Object.DestroyImmediate(action);
                UnityEngine.Object.DestroyImmediate(inner);
                UnityEngine.Object.DestroyImmediate(middle);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void NoOverridesAndIndependentSettings() {
            var scenario = ScriptableObject.CreateInstance<Scenario>();
            var container = ScriptableObject.CreateInstance<ScenarioContainer>();
            var action = ScriptableObject.CreateInstance<Entities>();
            try {
                scenario.Steps.Add(new ScenarioStep { Entities = action });
                container.Scenarios.Add(scenario);
                var steps = ScenarioRunner.Plan(container);
                Assert.AreEqual(ScenarioRunner.DefaultWaitFrames, steps[0].WaitFrames);
                Assert.AreEqual(ScenarioRunner.DefaultCaptureScreenshot, steps[0].CaptureScreenshot);

                scenario.OverrideWaitFrames = true;
                scenario.WaitFrames = 0;
                steps = ScenarioRunner.Plan(container);
                Assert.AreEqual(1, steps[0].WaitFrames, "Every action must allow at least one ECS/UI update frame.");
                scenario.WaitFrames = 12;
                scenario.OverrideScreenshot = true;
                scenario.CaptureScreenshot = true;
                steps = ScenarioRunner.Plan(container);
                Assert.AreEqual(12, steps[0].WaitFrames);
                Assert.IsTrue(steps[0].CaptureScreenshot);

                container.OverrideScreenshot = true;
                container.CaptureScreenshot = false;
                steps = ScenarioRunner.Plan(container);
                Assert.AreEqual(12, steps[0].WaitFrames);
                Assert.IsFalse(steps[0].CaptureScreenshot);
            } finally {
                UnityEngine.Object.DestroyImmediate(action);
                UnityEngine.Object.DestroyImmediate(container);
                UnityEngine.Object.DestroyImmediate(scenario);
            }
        }

        [Test]
        public void NestedOrderFilenamesAndCycles() {
            var root = ScriptableObject.CreateInstance<ScenarioContainer>();
            var child = ScriptableObject.CreateInstance<ScenarioContainer>();
            var first = ScriptableObject.CreateInstance<Scenario>();
            var second = ScriptableObject.CreateInstance<Scenario>();
            var one = ScriptableObject.CreateInstance<Entities>();
            var two = ScriptableObject.CreateInstance<Entities>();
            try {
                one.name = "OpenFeedback";
                two.name = "SetGerman";
                first.Steps.Add(new ScenarioStep { Entities = one });
                second.Steps.Add(new ScenarioStep { Entities = two });
                child.Scenarios.Add(first);
                root.Scenarios.Add(child);
                root.Scenarios.Add(second);
                var steps = ScenarioRunner.Plan(root);
                Assert.AreEqual(2, steps.Count);
                Assert.AreEqual("001_OpenFeedback.png", ScenarioRunner.ScreenshotFileName(0, steps[0].ScreenshotName));
                Assert.AreEqual("002_SetGerman.png", ScenarioRunner.ScreenshotFileName(1, steps[1].ScreenshotName));
                child.Scenarios.Add(root);
                Assert.Throws<InvalidOperationException>(() => ScenarioRunner.Plan(root));
            } finally {
                UnityEngine.Object.DestroyImmediate(one);
                UnityEngine.Object.DestroyImmediate(two);
                UnityEngine.Object.DestroyImmediate(second);
                UnityEngine.Object.DestroyImmediate(first);
                UnityEngine.Object.DestroyImmediate(child);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
