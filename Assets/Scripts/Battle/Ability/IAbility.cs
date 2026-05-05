using Battle.Core;

namespace Battle.Ability
{
    public interface IAbility
    {
        ActorId Owner { get; }

        AbilityHandle Handle { get; }

        CombatPhase Phase { get; }

        bool IsActive { get; }

        void OnAttach(CombatWorld world, ActorId owner, AbilityHandle handle);

        void OnDetach(CombatWorld world);

        void Tick(CombatWorld world, in CombatTime time);
    }
}
