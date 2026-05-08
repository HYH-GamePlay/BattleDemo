using System.Collections.Generic;
using MessagePack;

namespace Battle.CombatInfo.Action
{
    [MessagePackObject(true)]
    public sealed class ApplyEffectData : InstructData
    {
        public override InstructType id => InstructType.ApplyEffect;

        public List<int> effects = new List<int>();
    }
}
