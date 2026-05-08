using System.Collections.Generic;
using Battle.CombatInfo.State;
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

        public uint beginFrame;

        public uint endFrame;

        public ActorStateId requiredState;

        public ActorStateId blockedState;

        public List<int> requiredTags = new List<int>();

        public List<int> blockedTags = new List<int>();
    }
}
