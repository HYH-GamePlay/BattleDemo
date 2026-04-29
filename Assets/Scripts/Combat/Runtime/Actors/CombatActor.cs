using Combat.Runtime.Attributes;
using Combat.Runtime.Core;
using Combat.Runtime.Tags;

namespace Combat.Runtime.Actors
{
    public sealed class CombatActor
    {
        public string ActorIdentifier { get; }
        public string DisplayName { get; }
        public string TeamIdentifier { get; }
        public CombatVector3 Position;
        public AttributeSet Attributes { get; } = new();
        public GameplayTagContainer Tags { get; } = new();

        public bool IsDead => Tags.Has(CombatTags.StateDead);
        public float Health => Attributes.GetCurrentValue(CombatAttributes.Health);
        public float MaxHealth => Attributes.GetValue(CombatAttributes.MaxHealth);
        public float Stamina => Attributes.GetCurrentValue(CombatAttributes.Stamina);
        public float MaxStamina => Attributes.GetValue(CombatAttributes.MaxStamina);

        public CombatActor(CombatActorDefinition definition)
        {
            ActorIdentifier = definition.ActorIdentifier;
            DisplayName = definition.DisplayName;
            TeamIdentifier = definition.TeamIdentifier;
            Position = definition.InitialPosition;

            Attributes.SetBaseValue(CombatAttributes.MaxHealth, definition.MaxHealth);
            Attributes.SetBaseValue(CombatAttributes.MaxStamina, definition.MaxStamina);
            Attributes.SetBaseValue(CombatAttributes.StaminaRecovery, definition.StaminaRecovery);
            Attributes.SetBaseValue(CombatAttributes.AttackPower, definition.AttackPower);
            Attributes.SetBaseValue(CombatAttributes.Defense, definition.Defense);
            Attributes.SetBaseValue(CombatAttributes.CriticalChance, definition.CriticalChance);
            Attributes.SetBaseValue(CombatAttributes.CriticalMultiplier, definition.CriticalMultiplier);
            Attributes.SetBaseValue(CombatAttributes.DamageDealtMultiplier, definition.DamageDealtMultiplier);
            Attributes.SetBaseValue(CombatAttributes.DamageTakenMultiplier, definition.DamageTakenMultiplier);
            Attributes.SetBaseValue(CombatAttributes.MoveSpeed, definition.MoveSpeed);
            Attributes.SetBaseValue(CombatAttributes.AbilityCooldownMultiplier, definition.AbilityCooldownMultiplier);

            Attributes.SetCurrentValue(CombatAttributes.Health, definition.MaxHealth);
            Attributes.SetCurrentValue(CombatAttributes.Stamina, definition.MaxStamina);
        }
    }
}
