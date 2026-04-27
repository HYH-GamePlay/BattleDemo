using System.Collections.Generic;
using Combat.Core.Processors;

namespace Combat.Core
{
    /// <summary>
    /// 伤害流水线 — 责任链模式，处理器按注册顺序执行。
    /// 新增伤害修正只需实现 IHitProcessor 并注册，无需修改现有代码。
    /// </summary>
    public class HitPipeline
    {
        private readonly List<IHitProcessor> _processors;

        public HitPipeline(cfg.Config.ConstantCfg constants, Buff.BuffSystem buffSystem)
        {
            _processors = new List<IHitProcessor>
            {
                new AttackPowerProcessor(),
                new CritProcessor(),
                new DefenseProcessor(constants),
                new BuffModifierProcessor(buffSystem),
                new ApplyDamageProcessor(),
            };
        }

        public HitResult Process(HitContext ctx)
        {
            var result = new HitResult();
            foreach (var p in _processors)
                p.Process(ref ctx, ref result);
            return result;
        }
    }
}
