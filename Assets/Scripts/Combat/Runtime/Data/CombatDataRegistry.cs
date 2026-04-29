using System.Collections.Generic;
using Combat.Runtime.Abilities;
using Combat.Runtime.Effects;
using Combat.Runtime.Hitboxes;
using Combat.Runtime.Timeline;

namespace Combat.Runtime.Data
{
    public sealed class CombatDataRegistry
    {
        private readonly Dictionary<string, AbilityDefinition> _abilities = new();
        private readonly Dictionary<string, EffectDefinition> _effects = new();
        private readonly Dictionary<string, TimelineDefinition> _timelines = new();
        private readonly Dictionary<string, HitboxDefinition> _hitboxes = new();

        public void RegisterAbility(AbilityDefinition definition)
        {
            if (definition != null && !string.IsNullOrEmpty(definition.AbilityIdentifier))
                _abilities[definition.AbilityIdentifier] = definition;
        }

        public void RegisterEffect(EffectDefinition definition)
        {
            if (definition != null && !string.IsNullOrEmpty(definition.EffectIdentifier))
                _effects[definition.EffectIdentifier] = definition;
        }

        public void RegisterTimeline(TimelineDefinition definition)
        {
            if (definition != null && !string.IsNullOrEmpty(definition.TimelineIdentifier))
                _timelines[definition.TimelineIdentifier] = definition;
        }

        public void RegisterHitbox(HitboxDefinition definition)
        {
            if (definition != null && !string.IsNullOrEmpty(definition.HitboxIdentifier))
                _hitboxes[definition.HitboxIdentifier] = definition;
        }

        public void Clear()
        {
            _abilities.Clear();
            _effects.Clear();
            _timelines.Clear();
            _hitboxes.Clear();
        }

        public bool TryGetAbility(string abilityIdentifier, out AbilityDefinition definition)
            => _abilities.TryGetValue(abilityIdentifier, out definition);

        public bool TryGetEffect(string effectIdentifier, out EffectDefinition definition)
            => _effects.TryGetValue(effectIdentifier, out definition);

        public bool TryGetTimeline(string timelineIdentifier, out TimelineDefinition definition)
            => _timelines.TryGetValue(timelineIdentifier, out definition);

        public bool TryGetHitbox(string hitboxIdentifier, out HitboxDefinition definition)
            => _hitboxes.TryGetValue(hitboxIdentifier, out definition);
    }
}
