using UnityEngine;

namespace Combat.Core.Processors
{
    public class AttackPowerProcessor : IHitProcessor
    {
        public void Process(ref HitContext ctx, ref HitResult result)
        {
            result.FinalDamage = ctx.BaseDamage
                * (ctx.Attacker?.Stats.Get(cfg.StatType.AttackPower) ?? 1f);
        }
    }

    public class CritProcessor : IHitProcessor
    {
        public void Process(ref HitContext ctx, ref HitResult result)
        {
            if (ctx.Attacker == null) return;
            if (Random.value < ctx.Attacker.Stats.Get(cfg.StatType.CritRate))
            {
                result.IsCrit = true;
                result.FinalDamage *= ctx.Attacker.Stats.Get(cfg.StatType.CritMultiplier);
            }
        }
    }

    public class DefenseProcessor : IHitProcessor
    {
        private readonly cfg.Config.ConstantCfg _formula;

        public DefenseProcessor(cfg.Config.ConstantCfg formula) => _formula = formula;

        public void Process(ref HitContext ctx, ref HitResult result)
        {
            if (ctx.Defender == null) return;
            var stateName = ctx.Defender.Fsm?.CurrentStateName;

            if (stateName == nameof(FSM.States.DeadState)) { result.FinalDamage = 0f; return; }

            if (stateName == nameof(FSM.States.DefendState))
            {
                var holdTime = 0f; // DefendState 通过 Blackboard 存储 holdTime
                result.WasBlocked = true;

                if (holdTime <= _formula.PerfectBlockWindow)
                {
                    result.FinalDamage = 0f;
                    result.WasPerfectBlock = true;
                    ctx.Defender.Fsm.TransitionTo<FSM.States.StunnedState>(); // 反击窗口复用 Stunned
                    ctx.Bus.Publish(new PerfectBlockEvent
                    {
                        Defender = ctx.Defender,
                        Attacker = ctx.Attacker
                    });
                }
                else
                {
                    result.FinalDamage *= _formula.DefenseReductionRatio;
                }
            }
        }
    }

    public class BuffModifierProcessor : IHitProcessor
    {
        private readonly Buff.BuffSystem _buffSystem;

        public BuffModifierProcessor(Buff.BuffSystem buffSystem) => _buffSystem = buffSystem;

        public void Process(ref HitContext ctx, ref HitResult result)
        {
            if (ctx.Defender == null) return;
            result.FinalDamage *= _buffSystem.GetDamageAmplify(ctx.Defender);
        }
    }

    public class ApplyDamageProcessor : IHitProcessor
    {
        public void Process(ref HitContext ctx, ref HitResult result)
        {
            if (ctx.Defender == null || result.FinalDamage <= 0f) return;
            ctx.Defender.Vitals.TakeDamage(result.FinalDamage, ctx.Attacker, ctx.Bus);
            if (ctx.Defender.Vitals.IsDead)
                ctx.Defender.Fsm.TransitionTo<FSM.States.DeadState>();
        }
    }
}
