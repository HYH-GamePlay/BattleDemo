using Battle.CombatInfo.Tag;
using Battle.Core;

namespace Battle.Behavior.Tag
{
    public sealed class TagBehavior : BehaviorBase
    {
        public override CombatPhase Phase => CombatPhase.State;

        public bool Add(CombatWorld world, ActorId actorId, TagId tag)
        {
            var tagInfoHandle = world.GetOrAddActorInfo<TagContainerInfo>(actorId);
            return Add(world, tagInfoHandle, tag);
        }

        public bool Remove(CombatWorld world, ActorId actorId, TagId tag)
        {
            return world.TryGetActorInfo(actorId, out InfoHandle tagInfoHandle, out TagContainerInfo _)
                && Remove(world, tagInfoHandle, tag);
        }

        public bool Has(CombatWorld world, ActorId actorId, TagId tag)
        {
            return world.TryGetActorInfo(actorId, out InfoHandle tagInfoHandle, out TagContainerInfo _)
                && Has(world, tagInfoHandle, tag);
        }

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

        public bool IsSatisfiedBy(CombatWorld world, ActorId actorId, TagRequirement requirement)
        {
            if (!world.TryGetActorInfo(actorId, out InfoHandle tagInfoHandle, out TagContainerInfo _))
            {
                return requirement != null && requirement.IsSatisfiedBy(null);
            }

            return IsSatisfiedBy(world, tagInfoHandle, requirement);
        }
    }
}
