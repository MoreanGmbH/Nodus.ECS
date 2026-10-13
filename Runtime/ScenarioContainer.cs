using System.Collections.Generic;
using UnityEngine;

namespace Nodus.ECS {
    [CreateAssetMenu(menuName = "Nodus/ECS/Scenario Container", fileName = "ScenarioContainer")]
    public sealed class ScenarioContainer : ScenarioAsset {
        [Tooltip("Ordered list of Scenarios or nested Scenario Containers. The outermost enabled overrides take priority.")]
        public List<ScenarioAsset> Scenarios = new();
    }
}
