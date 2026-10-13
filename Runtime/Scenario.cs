using System;
using System.Collections.Generic;
using UnityEngine;
using Entities = Nodus.ECS.Definitions.Entities;

namespace Nodus.ECS {
    [Serializable]
    public struct ScenarioStep {
        [Tooltip("Entities definition to execute. Timing and screenshots are configured on the Scenario or its parent Container.")]
        public Entities Entities;
    }

    [CreateAssetMenu(menuName = "Nodus/ECS/Scenario", fileName = "Scenario")]
    public sealed class Scenario : ScenarioAsset {
        [Tooltip("Ordered actions. Each step only selects the Entities asset to spawn.")]
        public List<ScenarioStep> Steps = new();
    }
}
