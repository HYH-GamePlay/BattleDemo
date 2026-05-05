using Battle.Ability.Effects;
using Battle.Core;

namespace Battle.CombatInfo.Effect
{
    public sealed class EffectRuntimeInfo : ICombatInfo
    {
        public EffectRuntimeInfo(EffectSpec spec)
        {
            Spec = spec;
            StackCount = spec != null ? spec.StackCount : 0;
        }

        public EffectSpec Spec { get; }

        public float ElapsedTime { get; set; }

        public float PeriodElapsedTime { get; set; }

        public int StackCount { get; set; }

        public bool IsExpired { get; set; }

        public ActorId SourceActor => Spec != null ? Spec.SourceActor : ActorId.Invalid;

        public ActorId TargetActor => Spec != null ? Spec.TargetActor : ActorId.Invalid;
    }
}
