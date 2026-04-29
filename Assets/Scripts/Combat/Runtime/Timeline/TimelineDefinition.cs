using System.Collections.Generic;
using Combat.Runtime.Core;

namespace Combat.Runtime.Timeline
{
    public enum TimelineEventTarget
    {
        Unspecified,
        Self,
        CurrentTarget,
        TargetPosition,
    }

    public enum TimelineMovementMode
    {
        Offset,
        AimDirection,
        TowardTargetPosition,
    }

    public enum TimelineEventType
    {
        RequestCue,
        ApplyEffectToSelf,
        ApplyEffectToTarget,
        DamageTarget,
        ActivateHitbox,
        AddTag,
        RemoveTag,
        MoveActor,
    }

    [System.Serializable]
    public sealed class TimelineEventDefinition
    {
        public float Time;
        public TimelineEventType EventType;
        public TimelineEventTarget Target = TimelineEventTarget.Unspecified;
        public TimelineMovementMode MovementMode = TimelineMovementMode.Offset;
        public string CueIdentifier;
        public string EffectIdentifier;
        public string HitboxIdentifier;
        public string Tag;
        public string DamageTypeTag;
        public float Value;
        public CombatVector3 Vector;
    }

    [System.Serializable]
    public sealed class TimelineDefinition
    {
        public string TimelineIdentifier;
        public float Length;
        public List<TimelineEventDefinition> Events = new();
    }
}
