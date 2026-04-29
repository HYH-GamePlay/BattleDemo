using Combat.Runtime.Core;

namespace Combat.Runtime.Actors
{
    [System.Serializable]
    public sealed class CombatActorDefinition
    {
        public string ActorIdentifier;
        public string DisplayName;
        public string TeamIdentifier;
        public CombatVector3 InitialPosition;
        public float MaxHealth;
        public float MaxStamina;
        public float StaminaRecovery;
        public float AttackPower;
        public float Defense;
        public float CriticalChance;
        public float CriticalMultiplier = 1.5f;
        public float DamageDealtMultiplier = 1f;
        public float DamageTakenMultiplier = 1f;
        public float MoveSpeed;
        public float AbilityCooldownMultiplier = 1f;
    }
}
