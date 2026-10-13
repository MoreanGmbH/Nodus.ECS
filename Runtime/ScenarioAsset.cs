using UnityEngine;

namespace Nodus.ECS {
    public abstract class ScenarioAsset : ScriptableObject {
        [Tooltip("Override the wait between actions. When unchecked, inherit from the parent; if no ancestor overrides it, use 60 frames.")]
        public bool OverrideWaitFrames;

        [Tooltip("Frames between actions. At least one frame is always required so ECS/UI systems can process the action.")]
        [Min(0)] public int WaitFrames = 2;

        [Tooltip("Override screenshot capture. When unchecked, inherit from the parent; without an override anywhere, screenshots are captured.")]
        public bool OverrideScreenshot;

        [Tooltip("Whether to capture screenshots after each action when Screenshot Override is enabled. Uncheck to explicitly disable capture.")]
        public bool CaptureScreenshot;
    }
}
