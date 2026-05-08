using Battle.CombatInfo.Action;

namespace Battle.Behavior.Action.Executors
{
    public sealed class CollisionExecutor : Executor<CollisionData>
    {
        protected override void OnExecute(in ActionExecutionContext context, CollisionData data)
        {
            if (data.effects == null || data.effects.Count == 0)
            {
                return;
            }

            var applyData = new ApplyEffectData
            {
                effects = data.effects,
                et = data.et,
            };

            ApplyEffectExecutor.ApplyEffects(context, applyData);
        }
    }
}
