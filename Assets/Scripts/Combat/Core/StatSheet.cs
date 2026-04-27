using System;
using System.Collections.Generic;

namespace Combat.Core
{
    /// <summary>
    /// 属性修改器
    /// </summary>
    public struct StatModifier
    {
        public cfg.StatType Type;
        public cfg.ModOp Op;
        public float Value;
        public object Source;

        public StatModifier(cfg.StatType type, cfg.ModOp op, float value, object source = null)
        {
            Type = type;
            Op = op;
            Value = value;
            Source = source;
        }
    }

    /// <summary>
    /// 运行时属性表
    /// 支持三层叠加：Base + Additive + Multiplicative
    /// </summary>
    public class StatSheet
    {
        private readonly Dictionary<cfg.StatType, float> _baseValues = new();
        private readonly List<StatModifier> _modifiers = new();

        public void SetBase(cfg.StatType type, float value)
        {
            _baseValues[type] = value;
        }

        public float GetBase(cfg.StatType type)
        {
            return _baseValues.TryGetValue(type, out var val) ? val : 0f;
        }

        public void AddModifier(StatModifier modifier)
        {
            _modifiers.Add(modifier);
        }

        public void RemoveModifiersFromSource(object source)
        {
            _modifiers.RemoveAll(m => m.Source == source);
        }

        public void ClearRunModifiers()
        {
            _modifiers.Clear();
        }

        /// <summary>
        /// 计算最终属性值
        /// 公式: (base + Σadditive) × Πmultiplicative
        /// </summary>
        public float Get(cfg.StatType type)
        {
            float baseVal = GetBase(type);

            float additive = 0f;
            float multiplier = 1f;

            foreach (var mod in _modifiers)
            {
                if (mod.Type != type) continue;

                if (mod.Op == cfg.ModOp.Add)
                    additive += mod.Value;
                else if (mod.Op == cfg.ModOp.Mul)
                    multiplier *= mod.Value;
            }

            return (baseVal + additive) * multiplier;
        }

        /// <summary>
        /// 从数据表初始化基础属性
        /// </summary>
        public void InitFromData(cfg.Config.ActorCfg actorCfg)
        {
            SetBase(cfg.StatType.MaxHp,            actorCfg.MaxHp);
            SetBase(cfg.StatType.Stamina,          actorCfg.Stamina);
            SetBase(cfg.StatType.StaminaRegen,     actorCfg.StaminaRegen);
            SetBase(cfg.StatType.AttackPower,      actorCfg.AttackPower);
            SetBase(cfg.StatType.Defense,          actorCfg.Defense);
            SetBase(cfg.StatType.CritRate,         actorCfg.CritRate);
            SetBase(cfg.StatType.CritMultiplier,   actorCfg.CritMultiplier);
            SetBase(cfg.StatType.MoveSpeed,        actorCfg.MoveSpeed);
            SetBase(cfg.StatType.SkillCooldownMult,actorCfg.SkillCooldownMult);
        }
    }
}
