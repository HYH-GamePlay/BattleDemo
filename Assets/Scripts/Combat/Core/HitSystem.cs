using System;
using UnityEngine;

namespace Combat.Core
{
    /// <summary>
    /// 命中上下文
    /// </summary>
    public struct HitContext
    {
        public float BaseDamage;
        public CombatEntity Attacker;
        public CombatEntity Defender;
        public bool IsPerfectBlock;
        public EventBus Bus;
    }
}
