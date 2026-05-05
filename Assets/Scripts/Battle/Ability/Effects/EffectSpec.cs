using Battle.CombatInfo.Effect;
using Battle.Core;

namespace Battle.Ability.Effects
{
    public sealed class EffectSpec
    {
        public EffectSpec(EffectData data)
        {
            Data = data;
        }

        public EffectData Data { get; }

        public ActorId SourceActor { get; set; } = ActorId.Invalid;

        public ActorId TargetActor { get; set; } = ActorId.Invalid;

        public AbilityHandle SourceAbility { get; set; } = AbilityHandle.Invalid;

        public int Level { get; set; } = 1;

        public int StackCount { get; set; } = 1;

        public int RandomSeed { get; set; }
    }
}
