using System.Collections.Generic;
using Battle.CombatInfo;
using Battle.Core;

namespace Battle.CombatInfo.Action
{
    public sealed class ActionRuntimeInfo : IBehaviorInfo
    {
        private readonly HashSet<int> _activeInstructs = new HashSet<int>();

        public ActionRuntimeInfo(long runtimeId, ActorId owner, int actionId, ActionData data)
        {
            RuntimeId = runtimeId;
            Owner = owner;
            ActionId = actionId;
            Data = data;
        }

        public long RuntimeId { get; }

        public ActorId Owner { get; }

        public int ActionId { get; }

        public ActionData Data { get; }

        public uint ElapsedFrames { get; set; }

        public bool IsFinished { get; set; }

        public bool IsCanceled { get; set; }

        public IReadOnlyCollection<int> ActiveInstructs => _activeInstructs;

        public bool MarkInstructActive(int index)
        {
            return _activeInstructs.Add(index);
        }

        public bool MarkInstructInactive(int index)
        {
            return _activeInstructs.Remove(index);
        }

        public bool IsInstructActive(int index)
        {
            return _activeInstructs.Contains(index);
        }

        public void ClearActiveInstructs()
        {
            _activeInstructs.Clear();
        }
    }
}
