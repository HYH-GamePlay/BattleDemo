using Battle.Core;

namespace Battle.Behavior
{
    public interface IBehavior
    {
        ActorId Owner { get; }

        BehaviorHandle Handle { get; }

        CombatPhase Phase { get; }

        bool IsActive { get; }

        void OnAttach(CombatWorld world, ActorId owner, BehaviorHandle handle);

        void OnDetach(CombatWorld world);

        void Tick(CombatWorld world, in CombatTime time);
    }
}
