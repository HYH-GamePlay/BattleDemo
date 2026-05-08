using MessagePack;

namespace Battle.CombatInfo.Action
{
    [MessagePackObject(true)]
    public abstract class InstructData
    {
        [IgnoreMember]
        public abstract InstructType id { get; }

        public ExecuteTarget et = ExecuteTarget.ActionOwner;
    }
}
