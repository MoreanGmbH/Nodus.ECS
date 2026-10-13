using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nodus.ECS {
    [Serializable]
    public struct ScenarioStep {
        public Nodus.ECS.Definitions.Entities Entities;
        [Min(0)] public int WaitFrames;
        public bool CaptureScreenshot;
        public string ScreenshotName;
    }

    [CreateAssetMenu(menuName = "Nodus/ECS/Scenario", fileName = "Scenario")]
    public sealed class Scenario : ScriptableObject {
        [SerializeField] public List<ScenarioStep> Steps = new();
    }
}
