using MessagePack;

namespace Battle.CombatInfo.Action
{
    [MessagePackObject(true)]
    public sealed class AnimationData : InstructData
    {
        public override InstructType id => InstructType.Animation;

        public int animation;
    }
}
