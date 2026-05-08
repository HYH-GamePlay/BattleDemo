using Battle.CombatInfo.Effect;
using Battle.Core;

namespace Battle.Behavior.Effects
{
    public class EffectBehavior : BehaviorBase
    {
        public override CombatPhase Phase => CombatPhase.Damage;

        public bool Apply(CombatWorld world, EffectSpec spec)
        {
            if (spec == null || !spec.TargetActor.IsValid)
            {
                return false;
            }

            var handle = world.GetOrAddActorInfo<EffectApplicationInfo>(spec.TargetActor);
            var info = world.GetInfo<EffectApplicationInfo>(handle);
            info.Add(spec);
            world.MarkInfoDirty(handle);
            return true;
        }
    }
}
