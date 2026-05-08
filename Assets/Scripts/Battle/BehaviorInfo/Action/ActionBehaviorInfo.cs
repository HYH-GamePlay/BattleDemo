using System.Collections.Generic;
using Battle.CombatInfo;

namespace Battle.CombatInfo.Action
{
    public sealed class ActionBehaviorInfo : IBehaviorInfo
    {
        private readonly List<ActionRuntimeInfo> _runtimes = new List<ActionRuntimeInfo>();

        public IReadOnlyList<ActionRuntimeInfo> Runtimes => _runtimes;

        public int PendingActionId { get; set; }

        public bool HasPendingAction => PendingActionId > 0;

        public void AddRuntime(ActionRuntimeInfo runtime)
        {
            if (runtime != null)
            {
                _runtimes.Add(runtime);
            }
        }

        public bool RemoveRuntime(ActionRuntimeInfo runtime)
        {
            return runtime != null && _runtimes.Remove(runtime);
        }

        public void ClearPendingAction()
        {
            PendingActionId = 0;
        }
    }
}
