using System.Collections.Generic;
using Combat.Runtime.Attributes;

namespace Combat.Runtime.Effects
{
    public enum EffectStackingPolicy
    {
        RefreshDuration,
        AddStack,
        Independent,
    }

    public enum EffectExecutionType
    {
        None,
        Damage,
        Heal,
        ApplyEffect,
        RequestCue,
    }

    [System.Serializable]
    public sealed class AttributeModifierDefinition
    {
        public string AttributeIdentifier;
        public AttributeModifierOperation Operation;
        public float Value;
    }

    [System.Serializable]
    public sealed class EffectExecutionDefinition
    {
        public EffectExecutionType ExecutionType;
        public float Value;
        public string EffectIdentifier;
        public string CueIdentifier;
        public string DamageTypeTag;
    }

    [System.Serializable]
    public sealed class EffectDefinition
    {
        public string EffectIdentifier;
        public string DisplayName;
        public float Duration;
        public float Period;
        public int MaxStacks = 1;
        public EffectStackingPolicy StackingPolicy = EffectStackingPolicy.RefreshDuration;
        public List<string> GrantedTags = new();
        public List<AttributeModifierDefinition> AttributeModifiers = new();
        public List<EffectExecutionDefinition> ExecutionsOnApply = new();
        public List<EffectExecutionDefinition> ExecutionsOnPeriod = new();
        public List<EffectExecutionDefinition> ExecutionsOnRemove = new();

        public bool IsInstant => Duration <= 0f;
    }
}
