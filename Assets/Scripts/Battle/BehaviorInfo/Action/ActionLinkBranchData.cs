using MessagePack;

namespace Battle.CombatInfo.Action
{
    [MessagePackObject(true)]
    public sealed class ActionLinkBranchData
    {
        public int input;

        public int nextAction;

        public int priority;

        public ActionTransitionPolicy transition;
    }
}
