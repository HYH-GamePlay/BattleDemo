using System.Collections.Generic;

namespace Combat.Skill
{
    public class SkillInstance
    {
        public SkillGraph Graph;
        public Core.CombatEntity Owner;
        public float Cooldown;
        public float MaxCooldown;
        public float StaminaCost;
        public bool IsReady => Cooldown <= 0f;

        public SkillInstance(SkillGraph graph, Core.CombatEntity owner)
        {
            Graph = graph; Owner = owner;
            MaxCooldown = graph?.cooldown ?? 1f;
            StaminaCost = graph?.staminaCost ?? 0f;
        }

        public void Tick(float delta)
        {
            if (Cooldown > 0f)
                Cooldown -= delta * Owner.Stats.Get(cfg.StatType.SkillCooldownMult);
        }
    }

    /// <summary>
    /// 技能系统 — 纯C#，Tick驱动，无 MonoBehaviour 依赖。
    /// </summary>
    public class SkillSystem
    {
        private readonly Dictionary<string, SkillInstance> _skills = new();
        private readonly List<SkillExecution> _activeExecutions = new();
        private readonly Core.HitPipeline _pipeline;
        private readonly Buff.BuffSystem _buffSystem;

        public SkillSystem(Core.HitPipeline pipeline, Buff.BuffSystem buffSystem)
        {
            _pipeline = pipeline;
            _buffSystem = buffSystem;
        }

        public void Tick(float delta)
        {
            foreach (var s in _skills.Values) s.Tick(delta);

            for (int i = _activeExecutions.Count - 1; i >= 0; i--)
            {
                _activeExecutions[i].Tick(delta);
                if (_activeExecutions[i].IsComplete) _activeExecutions.RemoveAt(i);
            }
        }

        public void AddSkill(SkillGraph graph, Core.CombatEntity owner)
        {
            if (graph == null || string.IsNullOrEmpty(graph.skillId)) return;
            _skills[graph.skillId] = new SkillInstance(graph, owner);
        }

        public bool CanUseSkill(string skillId)
            => _skills.TryGetValue(skillId, out var s) && s.IsReady;

        public void ExecuteSkill(string skillId, SkillContext ctx)
        {
            if (!_skills.TryGetValue(skillId, out var skill) || !skill.IsReady) return;
            if (!skill.Owner.Vitals.UseStamina(skill.StaminaCost)) return;
            skill.Cooldown = skill.MaxCooldown;
            _activeExecutions.Add(new SkillExecution(skill.Graph, ctx, _pipeline, _buffSystem));
        }

        /// <summary>直接执行技能图（不走冷却/耐力检查，供遗物系统调用）。</summary>
        public void ExecuteGraph(SkillGraph graph, SkillContext ctx)
        {
            if (graph == null) return;
            _activeExecutions.Add(new SkillExecution(graph, ctx, _pipeline, _buffSystem));
        }

        public void Clear() { _skills.Clear(); _activeExecutions.Clear(); }
    }
}
