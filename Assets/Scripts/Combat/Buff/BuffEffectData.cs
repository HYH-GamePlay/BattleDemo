using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Combat.Buff
{
    public enum BuffEffectType
    {
        ModifyStat,      // 修改属性（攻击力+20%）
        DamageOverTime,  // 持续伤害
        HealOverTime,    // 持续治疗
        DamageAmplify,   // 受伤害放大（伤害流水线消费）
        Invincible,      // 无敌帧
        Stun,            // 眩晕（锁定状态机）
    }

    [Serializable]
    public struct BuffEffectData
    {
        [HorizontalGroup, HideLabel]
        public BuffEffectType EffectType;

        [ShowIf("@EffectType == BuffEffectType.ModifyStat")]
        public cfg.StatType TargetStat;

        [ShowIf("@EffectType == BuffEffectType.ModifyStat")]
        public cfg.ModOp ModOp;

        [LabelText("Value / Multiplier")]
        public float Value;

        [ShowIf("@EffectType == BuffEffectType.DamageOverTime || EffectType == BuffEffectType.HealOverTime")]
        [LabelText("Tick Interval (s)")]
        public float TickInterval;
    }
}
