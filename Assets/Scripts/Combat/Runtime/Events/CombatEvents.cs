using Combat.Runtime.Actors;
using Combat.Runtime.Core;

namespace Combat.Runtime.Events
{
    public struct ActorCreatedEvent : ICombatEvent
    {
        public CombatActor Actor;
    }

    public struct ActorRemovedEvent : ICombatEvent
    {
        public string ActorIdentifier;
    }

    public struct AttributeChangedEvent : ICombatEvent
    {
        public string ActorIdentifier;
        public string AttributeIdentifier;
        public float OldValue;
        public float NewValue;
    }

    public struct TagsChangedEvent : ICombatEvent
    {
        public string ActorIdentifier;
        public string Tag;
        public bool Added;
    }

    public struct AbilityActivatedEvent : ICombatEvent
    {
        public string ActorIdentifier;
        public string AbilityIdentifier;
        public string TargetActorIdentifier;
    }

    public struct AbilityEndedEvent : ICombatEvent
    {
        public string ActorIdentifier;
        public string AbilityIdentifier;
    }

    public struct AbilityFailedEvent : ICombatEvent
    {
        public string ActorIdentifier;
        public string AbilityIdentifier;
        public string Reason;
    }

    public struct EffectAppliedEvent : ICombatEvent
    {
        public string SourceActorIdentifier;
        public string TargetActorIdentifier;
        public string EffectIdentifier;
        public int StackCount;
    }

    public struct EffectRemovedEvent : ICombatEvent
    {
        public string TargetActorIdentifier;
        public string EffectIdentifier;
    }

    public struct DamageResolvedEvent : ICombatEvent
    {
        public string SourceActorIdentifier;
        public string TargetActorIdentifier;
        public float Amount;
        public bool IsCritical;
        public string DamageTypeTag;
    }

    public struct ActorDeathEvent : ICombatEvent
    {
        public string ActorIdentifier;
        public string KillerActorIdentifier;
    }

    public struct CueRequestedEvent : ICombatEvent
    {
        public string CueIdentifier;
        public string SourceActorIdentifier;
        public string TargetActorIdentifier;
        public CombatVector3 Position;
    }
}
