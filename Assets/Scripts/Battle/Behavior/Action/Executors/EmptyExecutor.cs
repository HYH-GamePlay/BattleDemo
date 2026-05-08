using Battle.CombatInfo.Action;

namespace Battle.Behavior.Action.Executors
{
    public sealed class EmptyExecutor<T> : Executor<T> where T : InstructData
    {
    }
}
