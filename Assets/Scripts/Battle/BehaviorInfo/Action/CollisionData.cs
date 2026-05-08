using System.Collections.Generic;
using MessagePack;

namespace Battle.CombatInfo.Action
{
    [MessagePackObject(true)]
    public sealed class CollisionData : InstructData
    {
        public override InstructType id => InstructType.Collision;

        public int hitbox;

        public int targetFilter;

        public List<int> effects = new List<int>();
    }
}
