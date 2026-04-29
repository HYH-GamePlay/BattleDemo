using Combat.Runtime.Core;

namespace Combat.Runtime.Abilities
{
    public readonly struct AbilityActivationRequest
    {
        public readonly string ActorIdentifier;
        public readonly string AbilityIdentifier;
        public readonly string TargetActorIdentifier;
        public readonly CombatVector3 AimDirection;
        public readonly CombatVector3 TargetPosition;

        public AbilityActivationRequest(
            string actorIdentifier,
            string abilityIdentifier,
            string targetActorIdentifier,
            CombatVector3 aimDirection,
            CombatVector3 targetPosition)
        {
            ActorIdentifier = actorIdentifier;
            AbilityIdentifier = abilityIdentifier;
            TargetActorIdentifier = targetActorIdentifier;
            AimDirection = aimDirection;
            TargetPosition = targetPosition;
        }
    }
}
