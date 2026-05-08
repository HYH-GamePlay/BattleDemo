using Battle.Core;

namespace Battle.Behavior
{
    public abstract class BehaviorBase : IBehavior
    {
        public ActorId Owner { get; private set; } = ActorId.Invalid;

        public BehaviorHandle Handle { get; private set; } = BehaviorHandle.Invalid;

        public virtual CombatPhase Phase => CombatPhase.Action;

        public bool IsActive { get; private set; }

        public void OnAttach(CombatWorld world, ActorId owner, BehaviorHandle handle)
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
            Handle = BehaviorHandle.Invalid;
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
