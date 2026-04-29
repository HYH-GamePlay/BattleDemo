using System.Collections.Generic;
using Combat.Runtime.Abilities;
using Combat.Runtime.Data;
using Combat.Runtime.Effects;
using Combat.Runtime.Hitboxes;
using Combat.Runtime.Timeline;
using UnityEngine;

namespace Combat.UnityBridge
{
    [CreateAssetMenu(menuName = "Combat/Definition Catalog", fileName = "CombatDefinitionCatalog")]
    public sealed class CombatDefinitionCatalog : ScriptableObject
    {
        [SerializeField] private List<AbilityDefinition> abilities = new();
        [SerializeField] private List<EffectDefinition> effects = new();
        [SerializeField] private List<TimelineDefinition> timelines = new();
        [SerializeField] private List<HitboxDefinition> hitboxes = new();

        public IReadOnlyList<AbilityDefinition> Abilities => abilities;
        public IReadOnlyList<EffectDefinition> Effects => effects;
        public IReadOnlyList<TimelineDefinition> Timelines => timelines;
        public IReadOnlyList<HitboxDefinition> Hitboxes => hitboxes;

        public void RegisterDefinitions(CombatDataRegistry registry)
        {
            if (registry == null) return;

            foreach (var ability in abilities)
                registry.RegisterAbility(ability);
            foreach (var effect in effects)
                registry.RegisterEffect(effect);
            foreach (var timeline in timelines)
                registry.RegisterTimeline(timeline);
            foreach (var hitbox in hitboxes)
                registry.RegisterHitbox(hitbox);
        }
    }
}
