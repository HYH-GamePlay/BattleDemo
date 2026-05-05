using Battle.Core;

namespace Battle.Ability
{
    public abstract class AbilityBase : IAbility
    {
        public ActorId Owner { get; private set; } = ActorId.Invalid;

        public AbilityHandle Handle { get; private set; } = AbilityHandle.Invalid;

        public virtual CombatPhase Phase => CombatPhase.Ability;

        public bool IsActive { get; private set; }

        public void OnAttach(CombatWorld world, ActorId owner, AbilityHandle handle)
        {
            Owner = owner;
            Handle = handle;
            IsActive = true;
            OnAttached(world);
        }

        public void OnDetach(CombatWorld world)
        {
            if (!IsActive)
            {
                return;
            }

            OnDetached(world);
            IsActive = false;
            Owner = ActorId.Invalid;
            Handle = AbilityHandle.Invalid;
        }

        public void Tick(CombatWorld world, in CombatTime time)
        {
            if (!IsActive)
            {
                return;
            }

            OnTick(world, time);
        }

        protected virtual void OnAttached(CombatWorld world)
        {
        }

        protected virtual void OnDetached(CombatWorld world)
        {
        }

        protected virtual void OnTick(CombatWorld world, in CombatTime time)
        {
        }
    }
}
