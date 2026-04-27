using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Core;

namespace Combat.Rogue
{
    public class RelicInstance
    {
        public cfg.Config.RelicCfg Data;
        public int Stacks;
        public Skill.SkillGraph CachedGraph;

        public RelicInstance(cfg.Config.RelicCfg data)
        {
            Data = data;
            Stacks = 1;
        }
    }

    /// <summary>
    /// 遗物系统
    /// </summary>
    public class RelicSystem
    {
        private readonly Core.EventBus _eventBus;
        private readonly Skill.SkillSystem _skillSystem;
        private readonly Dictionary<string, List<RelicInstance>> _entityRelics = new();

        public RelicSystem(Core.EventBus eventBus, Skill.SkillSystem skillSystem)
        {
            _eventBus = eventBus;
            _skillSystem = skillSystem;

            // 订阅战斗事件
            _eventBus.Subscribe<Core.HitEvent>(OnHitEvent);
            _eventBus.Subscribe<Core.KillEvent>(OnKillEvent);
            _eventBus.Subscribe<Core.PerfectBlockEvent>(OnPerfectBlockEvent);
        }

        public async void AddRelic(Core.CombatEntity entity, cfg.Config.RelicCfg relicData)
        {
            var entityId = entity.EntityId;
            if (!_entityRelics.TryGetValue(entityId, out var relicList))
            {
                relicList = new List<RelicInstance>();
                _entityRelics[entityId] = relicList;
            }

            var existing = relicList.Find(r => r.Data.RelicId == relicData.RelicId);
            if (existing != null)
            {
                existing.Stacks++;
            }
            else
            {
                var instance = new RelicInstance(relicData);
                relicList.Add(instance);

                if (!string.IsNullOrEmpty(relicData.TriggeredEffectPath))
                    instance.CachedGraph = await Game.Resource.LoadAsync<Skill.SkillGraph>(
                        relicData.TriggeredEffectPath);
            }

            foreach (var mod in relicData.PassiveModifiers)
                entity.Stats.AddModifier(new Core.StatModifier(mod.TargetStat, mod.ModOp, mod.Value, relicData));
        }

        public void RemoveRelic(Core.CombatEntity entity, string relicId)
        {
            var entityId = entity.EntityId;
            if (!_entityRelics.TryGetValue(entityId, out var relicList)) return;

            var relic = relicList.Find(r => r.Data.RelicId == relicId);
            if (relic != null)
            {
                // 移除属性加成
                entity.Stats.RemoveModifiersFromSource(relic.Data);
                relicList.Remove(relic);
            }
        }

        public List<RelicInstance> GetRelics(Core.CombatEntity entity)
        {
            var entityId = entity.EntityId;
            return _entityRelics.TryGetValue(entityId, out var list) ? list : new List<RelicInstance>();
        }

        private void OnHitEvent(Core.HitEvent evt)
        {
            if (evt.Attacker == null) return;

            var relics = GetRelics(evt.Attacker);
            foreach (var relic in relics)
            {
                if (relic.Data.Trigger == cfg.RelicTrigger.OnHit)
                {
                    TriggerRelicEffect(relic, evt.Attacker, evt.Defender);
                }
            }
        }

        private void OnKillEvent(Core.KillEvent evt)
        {
            if (evt.Killer == null) return;

            var relics = GetRelics(evt.Killer);
            foreach (var relic in relics)
            {
                if (relic.Data.Trigger == cfg.RelicTrigger.OnKill)
                {
                    TriggerRelicEffect(relic, evt.Killer, evt.Victim);
                }
            }
        }

        private void OnPerfectBlockEvent(Core.PerfectBlockEvent evt)
        {
            if (evt.Defender == null) return;

            var relics = GetRelics(evt.Defender);
            foreach (var relic in relics)
            {
                if (relic.Data.Trigger == cfg.RelicTrigger.OnPerfectBlock)
                {
                    TriggerRelicEffect(relic, evt.Defender, evt.Attacker);
                }
            }
        }

        private void TriggerRelicEffect(RelicInstance relic, Core.CombatEntity owner, Core.CombatEntity target)
        {
            if (relic.CachedGraph == null) return;

            var ctx = new Skill.SkillContext { Caster = owner, Target = target };
            _skillSystem.ExecuteGraph(relic.CachedGraph, ctx);
        }
    }
}
