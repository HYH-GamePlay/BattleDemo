using Battle.CombatInfo.Action;

namespace Battle.Behavior.Action.Executors
{
    public sealed class ActionLinkExecutor : Executor<ActionLinkData>
    {
        protected override void OnExecute(in ActionExecutionContext context, ActionLinkData data)
        {
            context.Behavior.EvaluateActionLink(context.Runtime, data);
        }
    }
}
