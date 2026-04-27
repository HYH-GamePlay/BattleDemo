using UnityEngine;
using XNode;

namespace Combat.Skill
{
    /// <summary>
    /// 技能图 — XNode 图容器，纯数据，不含执行逻辑。
    /// 执行由 SkillExecution 的 Tick 驱动。
    /// </summary>
    [CreateAssetMenu(fileName = "SkillGraph", menuName = "Combat/Skill Graph")]
    public class SkillGraph : NodeGraph
    {
        public string skillId;
        public string displayName;
        public Sprite icon;
        public float cooldown = 1f;
        public float staminaCost;

        public SkillEntryNode GetEntryNode()
        {
            foreach (var node in nodes)
                if (node is SkillEntryNode entry) return entry;
            return null;
        }
    }

    /// <summary>
    /// 技能节点基类 — Tick 驱动，返回 true 表示本节点执行完毕。
    /// </summary>
    public abstract class SkillNodeBase : Node
    {
        [Input(connectionType = ConnectionType.Override)]  public float input;
        [Output(connectionType = ConnectionType.Override)] public float output;

        /// <summary>每帧调用，返回 true 时推进到下一节点。</summary>
        public abstract bool Tick(float delta, SkillContext ctx,
            Core.HitPipeline pipeline, Buff.BuffSystem buffSystem);

        /// <summary>节点开始执行时调用（重置内部计时器等）。</summary>
        public virtual void OnEnter(SkillContext ctx) { }

        public SkillNodeBase GetNextNode(string portName = "output")
        {
            var port = GetOutputPort(portName);
            if (port == null || !port.IsConnected) return null;
            return port.Connection.node as SkillNodeBase;
        }
    }

    [NodeTint("#4CAF50")]
    public class SkillEntryNode : SkillNodeBase
    {
        public override bool Tick(float delta, SkillContext ctx,
            Core.HitPipeline pipeline, Buff.BuffSystem buffSystem) => true;
    }

    [NodeTint("#F44336")]
    public class DamageNode : SkillNodeBase
    {
        public float damageMultiplier = 1f;

        public override bool Tick(float delta, SkillContext ctx,
            Core.HitPipeline pipeline, Buff.BuffSystem buffSystem)
        {
            if (ctx.Caster != null && ctx.Target != null)
                pipeline.Process(new Core.HitContext
                {
                    Attacker = ctx.Caster,
                    Defender = ctx.Target,
                    BaseDamage = ctx.Caster.Stats.Get(cfg.StatType.AttackPower) * damageMultiplier * ctx.DamageMultiplier,
                    Bus = ctx.Bus,
                });
            return true;
        }
    }

    [NodeTint("#2196F3")]
    public class HealNode : SkillNodeBase
    {
        public float healAmount = 10f;
        public bool scaleWithAttack;

        public override bool Tick(float delta, SkillContext ctx,
            Core.HitPipeline pipeline, Buff.BuffSystem buffSystem)
        {
            if (ctx.Caster != null)
            {
                var amount = scaleWithAttack
                    ? healAmount * ctx.Caster.Stats.Get(cfg.StatType.AttackPower)
                    : healAmount;
                ctx.Caster.Vitals.Heal(amount, ctx.Bus);
            }
            return true;
        }
    }

    [NodeTint("#9E9E9E")]
    public class WaitNode : SkillNodeBase
    {
        public float duration = 0.5f;
        private float _elapsed;

        public override void OnEnter(SkillContext ctx) => _elapsed = 0f;

        public override bool Tick(float delta, SkillContext ctx,
            Core.HitPipeline pipeline, Buff.BuffSystem buffSystem)
        {
            _elapsed += delta;
            return _elapsed >= duration;
        }
    }

    [NodeTint("#00BCD4")]
    public class BuffNode : SkillNodeBase
    {
        public int buffId;
        public bool applyToTarget = true;

        public override bool Tick(float delta, SkillContext ctx,
            Core.HitPipeline pipeline, Buff.BuffSystem buffSystem)
        {
            var target = applyToTarget ? ctx.Target : ctx.Caster;
            if (target != null && buffId > 0)
                buffSystem.AddBuff(target, buffId);
            return true;
        }
    }

    [NodeTint("#E91E63")]
    public class VFXNode : SkillNodeBase
    {
        public string vfxKey;

        public override bool Tick(float delta, SkillContext ctx,
            Core.HitPipeline pipeline, Buff.BuffSystem buffSystem)
        {
            ctx.Bus?.Publish(new Core.SkillVFXEvent
            {
                VfxKey = vfxKey,
                PosX = ctx.CastPosition.x,
                PosZ = ctx.CastPosition.z,
                TargetEntityId = ctx.Target?.EntityId,
            });
            return true;
        }
    }

    [NodeTint("#795548")]
    public class AudioNode : SkillNodeBase
    {
        public string audioKey;
        public float volume = 1f;

        public override bool Tick(float delta, SkillContext ctx,
            Core.HitPipeline pipeline, Buff.BuffSystem buffSystem)
        {
            ctx.Bus?.Publish(new Core.SkillAudioEvent
            {
                AudioKey = audioKey,
                Volume = volume,
                SourceEntityId = ctx.Caster?.EntityId,
            });
            return true;
        }
    }

    [NodeTint("#9C27B0")]
    public class DashNode : SkillNodeBase
    {
        public float distance = 3f;
        public float duration = 0.2f;
        private float _elapsed;
        private UnityEngine.Vector2 _start;
        private UnityEngine.Vector2 _end;

        public override void OnEnter(SkillContext ctx)
        {
            _elapsed = 0f;
            if (ctx.Caster == null) return;
            _start = new UnityEngine.Vector2(ctx.Caster.Position.X, ctx.Caster.Position.Z);
            var dir = new UnityEngine.Vector2(ctx.AimDirection.x, ctx.AimDirection.z).normalized;
            _end = _start + dir * distance;
        }

        public override bool Tick(float delta, SkillContext ctx,
            Core.HitPipeline pipeline, Buff.BuffSystem buffSystem)
        {
            if (ctx.Caster == null) return true;
            _elapsed += delta;
            var t = UnityEngine.Mathf.Clamp01(_elapsed / duration);
            var pos = UnityEngine.Vector2.Lerp(_start, _end, t);
            ctx.Caster.Position.Set(pos.x, pos.y);
            return _elapsed >= duration;
        }
    }

    [NodeTint("#FF9800")]
    public class ConditionNode : SkillNodeBase
    {
        public enum ConditionType { TargetHpBelow, CasterHpBelow, RandomChance, IsPerfectBlock }
        public ConditionType condition;
        public float threshold;

        [Output(connectionType = ConnectionType.Override)] public float trueOutput;
        [Output(connectionType = ConnectionType.Override)] public float falseOutput;

        public override bool Tick(float delta, SkillContext ctx,
            Core.HitPipeline pipeline, Buff.BuffSystem buffSystem) => true;

        public bool Evaluate(SkillContext ctx) => condition switch
        {
            ConditionType.TargetHpBelow  => ctx.Target != null && ctx.Target.Vitals.Hp / ctx.Target.Stats.Get(cfg.StatType.MaxHp) < threshold,
            ConditionType.CasterHpBelow  => ctx.Caster != null && ctx.Caster.Vitals.Hp / ctx.Caster.Stats.Get(cfg.StatType.MaxHp) < threshold,
            ConditionType.RandomChance   => UnityEngine.Random.value < threshold,
            ConditionType.IsPerfectBlock => ctx.IsPerfectBlock,
            _ => false,
        };

        public SkillNodeBase GetBranchNode(SkillContext ctx)
            => GetNextNode(Evaluate(ctx) ? "trueOutput" : "falseOutput");
    }
}
