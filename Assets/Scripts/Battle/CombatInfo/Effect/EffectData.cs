using Battle.Ability.Tag;
using Battle.CombatInfo.Tag;

namespace Battle.CombatInfo.Effect
{
    public sealed class EffectData
    {
        public int EffectId { get; set; }

        public EffectDurationPolicy DurationPolicy { get; set; }

        public float Duration { get; set; }

        public float Period { get; set; }

        public int MaxStack { get; set; } = 1;

        public TagRequirement ApplicationRequirement { get; } = new TagRequirement();

        public TagSet GrantedTags { get; } = new TagSet();

        public TagSet RemovedTags { get; } = new TagSet();
    }
}
