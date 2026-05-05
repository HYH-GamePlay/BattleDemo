using Battle.Ability;
using Battle.CombatInfo.Tag;
using Battle.Core;

namespace Battle.Ability.Tag
{
    public sealed class TagAbility : AbilityBase
    {
        public override CombatPhase Phase => CombatPhase.State;

        public bool Add(CombatWorld world, InfoHandle tagInfoHandle, TagId tag)
        {
            if (!world.TryGetInfo(tagInfoHandle, out TagContainerInfo tagInfo))
            {
                return false;
            }

            var changed = tagInfo.Tags.Add(tag);
            if (changed)
            {
                world.MarkInfoDirty(tagInfoHandle);
            }

            return changed;
        }

        public bool Remove(CombatWorld world, InfoHandle tagInfoHandle, TagId tag)
        {
            if (!world.TryGetInfo(tagInfoHandle, out TagContainerInfo tagInfo))
            {
                return false;
            }

            var changed = tagInfo.Tags.Remove(tag);
            if (changed)
            {
                world.MarkInfoDirty(tagInfoHandle);
            }

            return changed;
        }

        public bool Has(CombatWorld world, InfoHandle tagInfoHandle, TagId tag)
        {
            return world.TryGetInfo(tagInfoHandle, out TagContainerInfo tagInfo) && tagInfo.Tags.Contains(tag);
        }

        public bool IsSatisfiedBy(CombatWorld world, InfoHandle tagInfoHandle, TagRequirement requirement)
        {
            return requirement != null
                && world.TryGetInfo(tagInfoHandle, out TagContainerInfo tagInfo)
                && requirement.IsSatisfiedBy(tagInfo.Tags);
        }
    }
}
