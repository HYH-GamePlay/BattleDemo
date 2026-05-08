using System.Collections.Generic;
using MessagePack;

namespace Battle.CombatInfo.Action
{
    [MessagePackObject(true)]
    public sealed class ActionLinkData : InstructData
    {
        public override InstructType id => InstructType.ActionLink;

        public List<ActionLinkBranchData> branches = new List<ActionLinkBranchData>();

        public bool consumeInput = true;
    }
}
