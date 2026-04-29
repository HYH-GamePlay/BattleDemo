using System;
using System.Collections.Generic;
using Combat.Runtime.Abilities;
using Combat.Runtime.Actors;
using Combat.Runtime.Cues;
using Combat.Runtime.Damage;
using Combat.Runtime.Data;
using Combat.Runtime.Effects;
using Combat.Runtime.Events;
using Combat.Runtime.Hitboxes;
using Combat.Runtime.Timeline;

namespace Combat.Runtime.Core
{
    public sealed class CombatWorld
    {
        private readonly Dictionary<string, CombatActor> _actors = new();
        private readonly Dictionary<string, Dictionary<string, int>> _tagReferenceCountsByActor = new();
        private readonly List<CombatActor> _actorTickBuffer = new();
        private readonly Random _random;

        public CombatWorldOptions Options { get; }
        public CombatDataRegistry DataRegistry { get; } = new();
        public CombatEventBus Events { get; } = new();
        public AbilitySystem Abilities { get; }
        public EffectSystem Effects { get; }
        public TimelineSystem Timelines { get; }
        public HitboxSystem Hitboxes { get; }
        public DamageSystem Damage { get; }
        public CueSystem Cues { get; }
        public float ElapsedTime { get; private set; }

        public CombatWorld(CombatWorldOptions options = null)
        {
            Options = options ?? new CombatWorldOptions();
            _random = Options.RandomSeed == 0 ? new Random() : new Random(Options.RandomSeed);

            Cues = new CueSystem(this);
            Damage = new DamageSystem(this);
            Hitboxes = new HitboxSystem(this);
            Effects = new EffectSystem(this);
            Timelines = new TimelineSystem(this);
            Abilities = new AbilitySystem(this);
        }

        public CombatActor CreateActor(CombatActorDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (string.IsNullOrWhiteSpace(definition.ActorIdentifier))
                throw new ArgumentException("Actor identifier cannot be empty.", nameof(definition));

            var actor = new CombatActor(definition);
            _actors[actor.ActorIdentifier] = actor;
            Events.Publish(new ActorCreatedEvent { Actor = actor });
            PublishAttributeSnapshot(actor);
            return actor;
        }

        public bool RemoveActor(string actorIdentifier)
        {
            if (!_actors.ContainsKey(actorIdentifier)) return false;

            Timelines.CancelActorTimelines(actorIdentifier, false);
            Effects.RemoveAllInvolvingActor(actorIdentifier);
            Abilities.ClearActor(actorIdentifier);
            _tagReferenceCountsByActor.Remove(actorIdentifier);
            _actors.Remove(actorIdentifier);
            Events.Publish(new ActorRemovedEvent { ActorIdentifier = actorIdentifier });
            return true;
        }

        public bool TryGetActor(string actorIdentifier, out CombatActor actor)
        {
            if (string.IsNullOrEmpty(actorIdentifier))
            {
                actor = null;
                return false;
            }

            return _actors.TryGetValue(actorIdentifier, out actor);
        }

        public IReadOnlyCollection<CombatActor> Actors => _actors.Values;

        public void Tick(float delta)
        {
            if (delta <= 0f) return;

            ElapsedTime += delta;
            _actorTickBuffer.Clear();
            _actorTickBuffer.AddRange(_actors.Values);

            foreach (var actor in _actorTickBuffer)
                RecoverStamina(actor, delta);

            Abilities.Tick(delta);
            Effects.Tick(delta);
            Timelines.Tick(delta);
            _actorTickBuffer.Clear();
        }

        public float NextRandom01() => (float)_random.NextDouble();

        public bool AddTag(CombatActor actor, string tag)
        {
            if (actor == null || string.IsNullOrWhiteSpace(tag)) return false;

            var counts = GetTagReferenceCounts(actor.ActorIdentifier);
            counts.TryGetValue(tag, out var count);
            counts[tag] = count + 1;

            if (count > 0 || !actor.Tags.Add(tag)) return false;

            Events.Publish(new TagsChangedEvent { ActorIdentifier = actor.ActorIdentifier, Tag = tag, Added = true });
            return true;
        }

        public bool RemoveTag(CombatActor actor, string tag)
        {
            if (actor == null || string.IsNullOrWhiteSpace(tag)) return false;

            if (_tagReferenceCountsByActor.TryGetValue(actor.ActorIdentifier, out var counts) &&
                counts.TryGetValue(tag, out var count))
            {
                if (count > 1)
                {
                    counts[tag] = count - 1;
                    return false;
                }

                counts.Remove(tag);
            }

            if (!actor.Tags.Remove(tag)) return false;
            Events.Publish(new TagsChangedEvent { ActorIdentifier = actor.ActorIdentifier, Tag = tag, Added = false });
            return true;
        }

        public void ChangeCurrentAttribute(CombatActor actor, string attributeIdentifier, float delta, float minimum, float maximum)
        {
            if (actor == null) return;

            var oldValue = actor.Attributes.GetCurrentValue(attributeIdentifier);
            var newValue = actor.Attributes.ChangeCurrentValue(attributeIdentifier, delta, minimum, maximum);
            if (Math.Abs(oldValue - newValue) <= 0.0001f) return;

            Events.Publish(new AttributeChangedEvent
            {
                ActorIdentifier = actor.ActorIdentifier,
                AttributeIdentifier = attributeIdentifier,
                OldValue = oldValue,
                NewValue = newValue,
            });
        }

        public void ClampVitalAttributes(CombatActor actor)
        {
            if (actor == null) return;

            ClampCurrentAttribute(actor, CombatAttributes.Health, 0f, actor.MaxHealth);
            ClampCurrentAttribute(actor, CombatAttributes.Stamina, 0f, actor.MaxStamina);
        }

        public void SetActorPosition(string actorIdentifier, CombatVector3 position)
        {
            if (_actors.TryGetValue(actorIdentifier, out var actor))
                actor.Position = position;
        }

        private void RecoverStamina(CombatActor actor, float delta)
        {
            if (actor == null || actor.IsDead) return;

            var recovery = actor.Attributes.GetValue(CombatAttributes.StaminaRecovery);
            if (recovery <= 0f) return;

            ChangeCurrentAttribute(
                actor,
                CombatAttributes.Stamina,
                recovery * delta,
                0f,
                actor.Attributes.GetValue(CombatAttributes.MaxStamina));
        }

        private void PublishAttributeSnapshot(CombatActor actor)
        {
            Events.Publish(new AttributeChangedEvent
            {
                ActorIdentifier = actor.ActorIdentifier,
                AttributeIdentifier = CombatAttributes.Health,
                OldValue = actor.Health,
                NewValue = actor.Health,
            });
            Events.Publish(new AttributeChangedEvent
            {
                ActorIdentifier = actor.ActorIdentifier,
                AttributeIdentifier = CombatAttributes.Stamina,
                OldValue = actor.Stamina,
                NewValue = actor.Stamina,
            });
        }

        private Dictionary<string, int> GetTagReferenceCounts(string actorIdentifier)
        {
            if (!_tagReferenceCountsByActor.TryGetValue(actorIdentifier, out var counts))
            {
                counts = new Dictionary<string, int>();
                _tagReferenceCountsByActor[actorIdentifier] = counts;
            }

            return counts;
        }

        private void ClampCurrentAttribute(CombatActor actor, string attributeIdentifier, float minimum, float maximum)
        {
            var oldValue = actor.Attributes.GetCurrentValue(attributeIdentifier);
            var clampedValue = CombatMath.Clamp(oldValue, minimum, maximum);
            if (Math.Abs(oldValue - clampedValue) <= 0.0001f) return;

            actor.Attributes.SetCurrentValue(attributeIdentifier, clampedValue);
            Events.Publish(new AttributeChangedEvent
            {
                ActorIdentifier = actor.ActorIdentifier,
                AttributeIdentifier = attributeIdentifier,
                OldValue = oldValue,
                NewValue = clampedValue,
            });
        }
    }
}
