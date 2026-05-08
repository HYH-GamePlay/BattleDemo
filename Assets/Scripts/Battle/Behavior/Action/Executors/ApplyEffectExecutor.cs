using Battle.Behavior.Effects;
using Battle.CombatInfo.Action;

namespace Battle.Behavior.Action.Executors
{
    public sealed class ApplyEffectExecutor : Executor<ApplyEffectData>
    {
        protected override void OnExecute(in ActionExecutionContext context, ApplyEffectData data)
        {
            ApplyEffects(context, data);
        }

        internal static void ApplyEffects(in ActionExecutionContext context, ApplyEffectData data)
        {
            if (data.effects == null || data.effects.Count == 0)
            {
                return;
            }

            if (!context.World.TryGetBehavior(context.Owner, out EffectBehavior effectBehavior))
            {
                return;
            }

            for (var i = 0; i < data.effects.Count; i++)
            {
                if (!context.Behavior.TryGetEffectData(data.effects[i], out var effectData))
                {
                    continue;
                }

                var spec = new EffectSpec(effectData)
                {
                    SourceActor = context.Owner,
                    TargetActor = context.Owner,
                    SourceBehavior = context.Behavior.Handle,
                    SourceActionRuntimeId = context.Runtime.RuntimeId,
                };

                effectBehavior.Apply(context.World, spec);
            }
        }
    }
}
