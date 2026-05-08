using Battle.CombatInfo.Action;
using Battle.CombatInfo.Tag;
using Battle.Behavior.Tag;

namespace Battle.Behavior.Action.Executors
{
    public sealed class AddTagExecutor : Executor<AddTagData>
    {
        protected override void OnEnter(in ActionExecutionContext context, AddTagData data)
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
                    tagBehavior.Add(context.World, context.Owner, tag);
                }
            }
        }

        protected override void OnExit(in ActionExecutionContext context, AddTagData data)
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
