using System.Collections.Generic;
using Combat.Runtime.Core;
using Combat.Runtime.Effects;
using Combat.Runtime.Events;

namespace Combat.Runtime.Abilities
{
    public sealed class AbilitySystem
    {
        private readonly CombatWorld _world;
        private readonly Dictionary<string, List<AbilitySpec>> _abilitiesByActor = new();

        public AbilitySystem(CombatWorld world)
        {
            _world = world;
        }

        public bool GrantAbility(string actorIdentifier, string abilityIdentifier)
        {
            if (!_world.DataRegistry.TryGetAbility(abilityIdentifier, out var definition)) return false;
            if (FindSpec(actorIdentifier, abilityIdentifier) != null) return true;

            GetList(actorIdentifier).Add(new AbilitySpec(definition));
            return true;
        }

        public bool RevokeAbility(string actorIdentifier, string abilityIdentifier)
        {
            if (!_abilitiesByActor.TryGetValue(actorIdentifier, out var list)) return false;
            var removed = list.RemoveAll(spec => spec.Definition.AbilityIdentifier == abilityIdentifier) > 0;
            if (list.Count == 0)
                _abilitiesByActor.Remove(actorIdentifier);

            return removed;
        }

        public bool Activate(AbilityActivationRequest request)
        {
            if (!_world.TryGetActor(request.ActorIdentifier, out var actor))
                return Fail(request, "Actor not found.");

            var spec = FindSpec(request.ActorIdentifier, request.AbilityIdentifier);
            if (spec == null)
                return Fail(request, "Ability not granted.");

            var definition = spec.Definition;
            if (!spec.IsReady)
                return Fail(request, "Ability is cooling down.");
            if (!actor.Tags.HasAll(definition.RequiredTags))
                return Fail(request, "Required tags are missing.");
            if (actor.Tags.HasAny(definition.BlockedTags))
                return Fail(request, "Blocked by tags.");
            if (actor.Stamina < definition.StaminaCost)
                return Fail(request, "Not enough stamina.");

            _world.ChangeCurrentAttribute(actor, CombatAttributes.Stamina, -definition.StaminaCost, 0f, actor.MaxStamina);
            foreach (var tag in definition.GrantedTags)
                _world.AddTag(actor, tag);
            foreach (var effectIdentifier in definition.EffectsAppliedToSelfOnActivation)
                _world.Effects.Apply(new EffectApplicationRequest(actor.ActorIdentifier, actor.ActorIdentifier, effectIdentifier));

            spec.StartCooldown(actor.Attributes.GetValue(CombatAttributes.AbilityCooldownMultiplier));
            _world.Events.Publish(new AbilityActivatedEvent
            {
                ActorIdentifier = actor.ActorIdentifier,
                AbilityIdentifier = definition.AbilityIdentifier,
                TargetActorIdentifier = request.TargetActorIdentifier,
            });

            if (string.IsNullOrEmpty(definition.TimelineIdentifier) ||
                !_world.Timelines.Play(definition, request))
            {
                EndAbility(actor.ActorIdentifier, definition);
            }

            return true;
        }

        public void Tick(float delta)
        {
            foreach (var specs in _abilitiesByActor.Values)
                foreach (var spec in specs)
                    spec.Tick(delta);
        }

        public void EndAbility(string actorIdentifier, AbilityDefinition definition)
        {
            if (definition == null) return;

            if (_world.TryGetActor(actorIdentifier, out var actor))
                foreach (var tag in definition.GrantedTags)
                    _world.RemoveTag(actor, tag);

            _world.Events.Publish(new AbilityEndedEvent
            {
                ActorIdentifier = actorIdentifier,
                AbilityIdentifier = definition.AbilityIdentifier,
            });
        }

        public void ClearActor(string actorIdentifier)
        {
            _abilitiesByActor.Remove(actorIdentifier);
        }

        private List<AbilitySpec> GetList(string actorIdentifier)
        {
            if (!_abilitiesByActor.TryGetValue(actorIdentifier, out var list))
            {
                list = new List<AbilitySpec>();
                _abilitiesByActor[actorIdentifier] = list;
            }

            return list;
        }

        private AbilitySpec FindSpec(string actorIdentifier, string abilityIdentifier)
            => _abilitiesByActor.TryGetValue(actorIdentifier, out var list)
                ? list.Find(spec => spec.Definition.AbilityIdentifier == abilityIdentifier)
                : null;

        private bool Fail(AbilityActivationRequest request, string reason)
        {
            _world.Events.Publish(new AbilityFailedEvent
            {
                ActorIdentifier = request.ActorIdentifier,
                AbilityIdentifier = request.AbilityIdentifier,
                Reason = reason,
            });
            return false;
        }
    }
}
