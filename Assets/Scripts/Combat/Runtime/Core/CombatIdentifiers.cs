namespace Combat.Runtime.Core
{
    public static class CombatAttributes
    {
        public const string Health = "Attribute.Health";
        public const string MaxHealth = "Attribute.MaxHealth";
        public const string Stamina = "Attribute.Stamina";
        public const string MaxStamina = "Attribute.MaxStamina";
        public const string StaminaRecovery = "Attribute.StaminaRecovery";
        public const string AttackPower = "Attribute.AttackPower";
        public const string Defense = "Attribute.Defense";
        public const string CriticalChance = "Attribute.CriticalChance";
        public const string CriticalMultiplier = "Attribute.CriticalMultiplier";
        public const string DamageDealtMultiplier = "Attribute.DamageDealtMultiplier";
        public const string DamageTakenMultiplier = "Attribute.DamageTakenMultiplier";
        public const string MoveSpeed = "Attribute.MoveSpeed";
        public const string AbilityCooldownMultiplier = "Attribute.AbilityCooldownMultiplier";
    }

    public static class CombatTags
    {
        public const string StateDead = "State.Dead";
        public const string StateInvincible = "State.Invincible";
        public const string StateStunned = "State.Stunned";
        public const string StateActing = "State.Acting";
        public const string StateDodging = "State.Dodging";
        public const string StateDefending = "State.Defending";
    }
}
