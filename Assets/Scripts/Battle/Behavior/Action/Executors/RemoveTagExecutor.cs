using Battle.CombatInfo.Action;
using Battle.CombatInfo.Tag;
using Battle.Behavior.Tag;

namespace Battle.Behavior.Action.Executors
{
    public sealed class RemoveTagExecutor : Executor<RemoveTagData>
    {
        protected override void OnEnter(in ActionExecutionContext context, RemoveTagData data)
        {
            if (data.tags == null || data.tags.Count == 0)
            {
                return;
            }

            if (!context.World.TryGetBehavior(context.Owner, out TagBehavior tagBehavior))
            {
                return;
            }

            for (var i = 0; i < data.tags.Count; i++)
            {
                var tag = new TagId(data.tags[i]);
                if (tag.IsValid)
                {
                    tagBehavior.Remove(context.World, context.Owner, tag);
                }
            }
        }
    }
}
