using Battle.Core;

namespace Battle.Ability.Behavior
{
    public class BehaviorAbility : AbilityBase
    {
        protected override void OnAttached(CombatWorld world)
        {
            base.OnAttached(world);
            
            
        }

        protected override void OnDetached(CombatWorld world)
        {
            base.OnDetached(world);
        }

        protected override void OnTick(CombatWorld world, in CombatTime time)
        {
            base.OnTick(world, in time);
        }
    }
}