using System.Collections.Generic;
using MessagePack;

namespace Battle.CombatInfo.Action
{
    [MessagePackObject(true)]
    public sealed class AddTagData : InstructData
    {
        public override InstructType id => InstructType.AddTag;

        public List<int> tags = new List<int>();
    }

    [MessagePackObject(true)]
    public sealed class RemoveTagData : InstructData
    {
        public override InstructType id => InstructType.RemoveTag;

        public List<int> tags = new List<int>();
    }
}
