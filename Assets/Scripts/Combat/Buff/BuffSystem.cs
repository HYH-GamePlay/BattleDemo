using System;
using System.Collections.Generic;
using cfg.Config;

namespace Combat.Buff
{
    public class ActiveBuff
    {
        public BuffCfg Definition;
        public List<BuffEffectCfg> Effects;
        public float RemainingTime;
        public int Stacks;
        public float[] EffectTimers;

        public bool IsExpired => RemainingTime <= 0f;
        public bool IsPermanent => Definition.DefaultDuration < 0f;
    }

    public class BuffSystem
    {
        private readonly Core.EventBus _bus;
        private readonly cfg.Tables _tables;
        private readonly Dictionary<string, (Core.CombatEntity Entity, List<ActiveBuff> Buffs)> _data = new();

        public BuffSystem(Core.EventBus bus, cfg.Tables tables)
        {
            _bus = bus;
            _tables = tables;
        }

        public void Register(Core.CombatEntity entity)
            => _data[entity.EntityId] = (entity, new List<ActiveBuff>());

        public void Unregister(string entityId) => _data.Remove(entityId);

        public void AddBuff(Core.CombatEntity entity, int buffId)
        {
            if (!_data.TryGetValue(entity.EntityId, out var entry)) return;
            var def = _tables.TbBuff.GetOrDefault(buffId);
            if (def == null) return;

            var buffs = entry.Buffs;
            var existing = buffs.Find(b => b.Definition.BuffId == def.BuffId);
            if (existing != null)
            {
                existing.Stacks = Math.Min(existing.Stacks + 1, def.MaxStacks);
                existing.RemainingTime = def.DefaultDuration;
                return;
            }

            var effects = BuildEffects(def);
            var buff = new ActiveBuff
            {
                Definition = def,
                Effects = effects,
                RemainingTime = def.DefaultDuration,
                Stacks = 1,
                EffectTimers = new float[effects.Count],
            };
            buffs.Add(buff);
            ApplyStatModifiers(entity, buff);
        }

        public void RemoveBuff(Core.CombatEntity entity, string buffId)
        {
            if (!_data.TryGetValue(entity.EntityId, out var entry)) return;
            var idx = entry.Buffs.FindIndex(b => b.Definition.BuffId == buffId);
            if (idx < 0) return;
            RemoveStatModifiers(entity, entry.Buffs[idx]);
            entry.Buffs.RemoveAt(idx);
        }

        public void Tick(float delta)
        {
            foreach (var (_, entry) in _data)
            {
                var buffs = entry.Buffs;
                for (int i = buffs.Count - 1; i >= 0; i--)
                {
                    var buff = buffs[i];
                    TickEffects(entry.Entity, buff, delta);
                    if (buff.IsPermanent) continue;
                    buff.RemainingTime -= delta;
                    if (buff.IsExpired)
                    {
                        RemoveStatModifiers(entry.Entity, buff);
                        buffs.RemoveAt(i);
                    }
                }
            }
        }

        public float GetDamageAmplify(Core.CombatEntity defender)
        {
            if (!_data.TryGetValue(defender.EntityId, out var entry)) return 1f;
            float amplify = 1f;
            foreach (var buff in entry.Buffs)
                foreach (var effect in buff.Effects)
                    if (effect.EffectType == cfg.BuffEffectType.DamageAmplify)
                        amplify *= (1f + effect.Value * buff.Stacks);
            return amplify;
        }

        public List<ActiveBuff> GetBuffs(Core.CombatEntity entity)
            => _data.TryGetValue(entity.EntityId, out var entry) ? entry.Buffs : new List<ActiveBuff>();

        private List<BuffEffectCfg> BuildEffects(BuffCfg def)
        {
            var list = new List<BuffEffectCfg>(def.EffectIds.Count);
            foreach (var id in def.EffectIds)
            {
                var e = _tables.TbBuffEffect.GetOrDefault(id);
                if (e != null) list.Add(e);
            }
            return list;
        }

        private void ApplyStatModifiers(Core.CombatEntity entity, ActiveBuff buff)
        {
            foreach (var effect in buff.Effects)
                if (effect.EffectType == cfg.BuffEffectType.ModifyStat)
                    entity.Stats.AddModifier(new Core.StatModifier(
                        (cfg.StatType)(effect.TargetStat - 1),
                        (cfg.ModOp)(effect.ModOp - 1),
                        effect.Value, buff));
        }

        private void RemoveStatModifiers(Core.CombatEntity entity, ActiveBuff buff)
            => entity.Stats.RemoveModifiersFromSource(buff);

        private void TickEffects(Core.CombatEntity entity, ActiveBuff buff, float delta)
        {
            for (int i = 0; i < buff.Effects.Count; i++)
            {
                var effect = buff.Effects[i];
                if (effect.EffectType != cfg.BuffEffectType.DamageOverTime
                    && effect.EffectType != cfg.BuffEffectType.HealOverTime) continue;

                buff.EffectTimers[i] += delta;
                if (buff.EffectTimers[i] < effect.TickInterval) continue;
                buff.EffectTimers[i] = 0f;

                float amount = effect.Value * buff.Stacks;
                if (effect.EffectType == cfg.BuffEffectType.DamageOverTime)
                    entity.Vitals.TakeDamage(amount, null, _bus);
                else
                    entity.Vitals.Heal(amount, _bus);
            }
        }
    }
}
