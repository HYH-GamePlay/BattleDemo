using Battle.CombatInfo.Effect;
using Battle.Core;

namespace Battle.Behavior.Effects
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

        public BehaviorHandle SourceBehavior { get; set; } = BehaviorHandle.Invalid;

        public long SourceActionRuntimeId { get; set; }

        public int Level { get; set; } = 1;

        public int StackCount { get; set; } = 1;

        public int RandomSeed { get; set; }
    }
}
