using Battle.CombatInfo.Action;
using Battle.Core;

namespace Battle.Behavior.Action.Executors
{
    public readonly struct ActionExecutionContext
    {
        public ActionExecutionContext(
            CombatWorld world,
            ActionBehavior behavior,
            ActionRuntimeInfo runtime,
            Instruct instruct,
            int instructIndex,
            CombatTime time)
        {
            World = world;
            Behavior = behavior;
            Runtime = runtime;
            Instruct = instruct;
            InstructIndex = instructIndex;
            Time = time;
        }

        public CombatWorld World { get; }

        public ActionBehavior Behavior { get; }

        public ActionRuntimeInfo Runtime { get; }

        public Instruct Instruct { get; }

        public int InstructIndex { get; }

        public CombatTime Time { get; }

        public ActorId Owner => Runtime != null ? Runtime.Owner : ActorId.Invalid;
    }
}
