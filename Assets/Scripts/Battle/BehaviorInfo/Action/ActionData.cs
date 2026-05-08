using System.Collections.Generic;
using MessagePack;

namespace Battle.CombatInfo.Action
{
    [MessagePackObject(true)]
    public sealed class ActionData
    {
        public uint length { get; set; }

        public List<Instruct> instructs { get; set; } = new List<Instruct>();

        public uint GetLength()
        {
            if (length > 0)
            {
                return length;
            }

            uint max = 0;
            for (var i = 0; i < instructs.Count; i++)
            {
                var instruct = instructs[i];
                if (instruct != null && instruct.end > max)
                {
                    max = instruct.end;
                }
            }

            return max;
        }
    }
}
