using MessagePack;

namespace Battle.CombatInfo.Action
{
    [MessagePackObject(true)]
    public sealed class ActionRawData
    {
        public uint length { get; set; }

        public uint[] begin { get; set; }

        public uint[] end { get; set; }

        public ushort[] instructTypes { get; set; }

        public byte[][] instructData { get; set; }
    }
}
