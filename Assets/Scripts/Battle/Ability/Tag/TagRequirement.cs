using Battle.CombatInfo.Tag;

namespace Battle.Ability.Tag
{
    public sealed class TagRequirement
    {
        public TagSet RequiredAll { get; } = new TagSet();

        public TagSet RequiredAny { get; } = new TagSet();

        public TagSet BlockedAny { get; } = new TagSet();

        public bool IsSatisfiedBy(TagSet tags)
        {
            if (tags == null)
            {
                return RequiredAll.Count == 0 && RequiredAny.Count == 0 && BlockedAny.Count == 0;
            }

            if (!tags.ContainsAll(RequiredAll))
            {
                return false;
            }

            if (RequiredAny.Count > 0 && !tags.ContainsAny(RequiredAny))
            {
                return false;
            }

            if (tags.ContainsAny(BlockedAny))
            {
                return false;
            }

            return true;
        }
    }
}
