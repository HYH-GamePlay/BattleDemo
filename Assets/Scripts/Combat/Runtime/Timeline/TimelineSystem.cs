using System.Collections.Generic;
using Combat.Runtime.Abilities;
using Combat.Runtime.Core;
using Combat.Runtime.Cues;
using Combat.Runtime.Damage;
using Combat.Runtime.Effects;

namespace Combat.Runtime.Timeline
{
    public sealed class TimelineSystem
    {
        private readonly CombatWorld _world;
        private readonly List<ActiveTimeline> _activeTimelines = new();

        public TimelineSystem(CombatWorld world)
        {
            _world = world;
        }

        public bool Play(AbilityDefinition abilityDefinition, AbilityActivationRequest request)
        {
            if (abilityDefinition == null) return false;
            if (!_world.DataRegistry.TryGetTimeline(abilityDefinition.TimelineIdentifier, out var timelineDefinition))
                return false;

            var activeTimeline = new ActiveTimeline(abilityDefinition, timelineDefinition, request);
            _activeTimelines.Add(activeTimeline);
            ExecuteDueEvents(activeTimeline, 0f);
            return true;
        }

        public void Tick(float delta)
        {
            for (var i = _activeTimelines.Count - 1; i >= 0; i--)
            {
                var timeline = _activeTimelines[i];
                timeline.ElapsedTime += delta;
                ExecuteDueEvents(timeline, timeline.ElapsedTime);

                if (!timeline.IsComplete) continue;
                _activeTimelines.RemoveAt(i);
                _world.Abilities.EndAbility(timeline.Request.ActorIdentifier, timeline.AbilityDefinition);
            }
        }

        public void CancelActorTimelines(string actorIdentifier, bool publishAbilityEnd)
        {
            for (var i = _activeTimelines.Count - 1; i >= 0; i--)
            {
                var timeline = _activeTimelines[i];
                if (timeline.Request.ActorIdentifier != actorIdentifier &&
                    timeline.Request.TargetActorIdentifier != actorIdentifier)
                {
                    continue;
                }

                _activeTimelines.RemoveAt(i);
                if (publishAbilityEnd)
                    _world.Abilities.EndAbility(timeline.Request.ActorIdentifier, timeline.AbilityDefinition);
            }
        }

        private void ExecuteDueEvents(ActiveTimeline timeline, float elapsedTime)
        {
            while (timeline.NextEventIndex < timeline.Events.Count &&
                   timeline.Events[timeline.NextEventIndex].Time <= elapsedTime)
            {
                ExecuteEvent(timeline, timeline.Events[timeline.NextEventIndex]);
                timeline.NextEventIndex++;
            }
        }

        private void ExecuteEvent(ActiveTimeline timeline, TimelineEventDefinition eventDefinition)
        {
            switch (eventDefinition.EventType)
            {
                case TimelineEventType.RequestCue:
                    RequestCue(timeline, eventDefinition);
                    break;
                case TimelineEventType.ApplyEffectToSelf:
                    ApplyEffect(timeline.Request.ActorIdentifier, timeline.Request.ActorIdentifier, eventDefinition.EffectIdentifier);
                    break;
                case TimelineEventType.ApplyEffectToTarget:
                    ApplyEffect(timeline.Request.ActorIdentifier, ResolveActorIdentifier(timeline, eventDefinition, TimelineEventTarget.CurrentTarget), eventDefinition.EffectIdentifier);
                    break;
                case TimelineEventType.DamageTarget:
                    DamageTarget(timeline, eventDefinition);
                    break;
                case TimelineEventType.ActivateHitbox:
                    _world.Hitboxes.Activate(eventDefinition.HitboxIdentifier, timeline.Request.ActorIdentifier);
                    break;
                case TimelineEventType.AddTag:
                    ChangeTag(timeline, eventDefinition, true);
                    break;
                case TimelineEventType.RemoveTag:
                    ChangeTag(timeline, eventDefinition, false);
                    break;
                case TimelineEventType.MoveActor:
                    MoveActor(timeline, eventDefinition);
                    break;
            }
        }

        private void RequestCue(ActiveTimeline timeline, TimelineEventDefinition eventDefinition)
        {
            if (string.IsNullOrEmpty(eventDefinition.CueIdentifier)) return;

            var targetActorIdentifier = ResolveActorIdentifier(timeline, eventDefinition, TimelineEventTarget.CurrentTarget);
            var position = ResolvePosition(timeline, eventDefinition, targetActorIdentifier);
            _world.Cues.Request(new CueRequest(
                eventDefinition.CueIdentifier,
                timeline.Request.ActorIdentifier,
                targetActorIdentifier,
                position));
        }

        private void ApplyEffect(string sourceActorIdentifier, string targetActorIdentifier, string effectIdentifier)
        {
            if (string.IsNullOrEmpty(targetActorIdentifier) || string.IsNullOrEmpty(effectIdentifier)) return;
            _world.Effects.Apply(new EffectApplicationRequest(sourceActorIdentifier, targetActorIdentifier, effectIdentifier));
        }

        private void DamageTarget(ActiveTimeline timeline, TimelineEventDefinition eventDefinition)
        {
            var targetActorIdentifier = ResolveActorIdentifier(timeline, eventDefinition, TimelineEventTarget.CurrentTarget);
            if (string.IsNullOrEmpty(targetActorIdentifier)) return;

            _world.Damage.Resolve(new DamageRequest(
                timeline.Request.ActorIdentifier,
                targetActorIdentifier,
                eventDefinition.Value,
                eventDefinition.DamageTypeTag));
        }

        private void ChangeTag(ActiveTimeline timeline, TimelineEventDefinition eventDefinition, bool add)
        {
            var targetActorIdentifier = ResolveActorIdentifier(timeline, eventDefinition, TimelineEventTarget.Self);
            if (string.IsNullOrEmpty(targetActorIdentifier) || !_world.TryGetActor(targetActorIdentifier, out var actor)) return;

            if (add)
                _world.AddTag(actor, eventDefinition.Tag);
            else
                _world.RemoveTag(actor, eventDefinition.Tag);
        }

        private void MoveActor(ActiveTimeline timeline, TimelineEventDefinition eventDefinition)
        {
            var targetActorIdentifier = ResolveActorIdentifier(timeline, eventDefinition, TimelineEventTarget.Self);
            if (!_world.TryGetActor(targetActorIdentifier, out var actor)) return;

            var offset = eventDefinition.MovementMode switch
            {
                TimelineMovementMode.AimDirection => timeline.Request.AimDirection.Normalized * eventDefinition.Value,
                TimelineMovementMode.TowardTargetPosition => (timeline.Request.TargetPosition - actor.Position).Normalized * eventDefinition.Value,
                _ => eventDefinition.Vector,
            };

            actor.Position += offset;
        }

        private string ResolveActorIdentifier(
            ActiveTimeline timeline,
            TimelineEventDefinition eventDefinition,
            TimelineEventTarget defaultTarget)
        {
            var target = eventDefinition.Target == TimelineEventTarget.Unspecified
                ? defaultTarget
                : eventDefinition.Target;

            return target switch
            {
                TimelineEventTarget.Self => timeline.Request.ActorIdentifier,
                TimelineEventTarget.CurrentTarget => timeline.Request.TargetActorIdentifier,
                _ => null,
            };
        }

        private CombatVector3 ResolvePosition(
            ActiveTimeline timeline,
            TimelineEventDefinition eventDefinition,
            string targetActorIdentifier)
        {
            if (eventDefinition.Target == TimelineEventTarget.TargetPosition)
                return timeline.Request.TargetPosition + eventDefinition.Vector;

            if (!string.IsNullOrEmpty(targetActorIdentifier) &&
                _world.TryGetActor(targetActorIdentifier, out var target))
            {
                return target.Position + eventDefinition.Vector;
            }

            return _world.TryGetActor(timeline.Request.ActorIdentifier, out var source)
                ? source.Position + eventDefinition.Vector
                : eventDefinition.Vector;
        }

        private sealed class ActiveTimeline
        {
            public readonly AbilityDefinition AbilityDefinition;
            public readonly AbilityActivationRequest Request;
            public readonly List<TimelineEventDefinition> Events;
            public readonly float Length;
            public float ElapsedTime;
            public int NextEventIndex;

            public ActiveTimeline(
                AbilityDefinition abilityDefinition,
                TimelineDefinition timelineDefinition,
                AbilityActivationRequest request)
            {
                AbilityDefinition = abilityDefinition;
                Request = request;
                Length = timelineDefinition.Length;
                Events = new List<TimelineEventDefinition>(timelineDefinition.Events);
                Events.Sort((left, right) => left.Time.CompareTo(right.Time));
            }

            public bool IsComplete => ElapsedTime >= Length && NextEventIndex >= Events.Count;
        }
    }
}
