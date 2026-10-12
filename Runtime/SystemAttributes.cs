using System;

namespace Nodus.ECS {
    [Flags]
    public enum SystemPhase {
        Start = 1,
        Update = 2,
        FixedUpdate = 4,
        LateUpdate = 8,
        All = Start | Update | FixedUpdate | LateUpdate
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SystemPhaseAttribute : Attribute {
        public SystemPhase Phases { get; }

        public SystemPhaseAttribute(SystemPhase phases) => Phases = phases;
    }
}
