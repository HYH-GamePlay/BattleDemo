using System.Collections.Generic;

namespace Combat.Runtime.Abilities
{
    [System.Serializable]
    public sealed class AbilityDefinition
    {
        public string AbilityIdentifier;
        public string DisplayName;
        public string TimelineIdentifier;
        public float Cooldown;
        public float StaminaCost;
        public List<string> RequiredTags = new();
        public List<string> BlockedTags = new();
        public List<string> GrantedTags = new();
        public List<string> EffectsAppliedToSelfOnActivation = new();
    }

    public sealed class AbilitySpec
    {
        public AbilityDefinition Definition { get; }
        public float RemainingCooldown { get; private set; }

        public AbilitySpec(AbilityDefinition definition)
        {
            Definition = definition;
        }

        public bool IsReady => RemainingCooldown <= 0f;

        public void StartCooldown(float cooldownMultiplier)
        {
            var multiplier = cooldownMultiplier <= 0f ? 1f : cooldownMultiplier;
            RemainingCooldown = Definition.Cooldown / multiplier;
        }

        public void Tick(float delta)
        {
            if (RemainingCooldown > 0f)
                RemainingCooldown -= delta;
        }
    }
}
