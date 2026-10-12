using Friflo.Engine.ECS;

namespace Nodus.ECS {
    [ComponentKey("Nodus.ECS.CommandStatus")]
    public struct CommandStatus : IComponent {
        public bool Handled;
    }
}
