using MessagePack;

namespace Battle.CombatInfo.Action
{
    [MessagePackObject(true)]
    public sealed class Instruct
    {
        public uint begin { get; set; }

        public uint end { get; set; }

        public InstructData data { get; set; }

        public bool IsValid => data != null && begin < end;
    }
}
