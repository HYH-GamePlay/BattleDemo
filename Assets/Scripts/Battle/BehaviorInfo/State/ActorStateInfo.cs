using System;
using Battle.CombatInfo;

namespace Battle.CombatInfo.State
{
    public sealed class ActorStateInfo : IBehaviorInfo
    {
        public ActorStateId CurrentState { get; private set; } = ActorStateId.None;

        public ActorStateId PreviousState { get; private set; } = ActorStateId.None;

        public uint StateFrame { get; private set; }

        public TimeSpan StateTime { get; private set; }

        public int RequestedActionId { get; private set; }

        public int CurrentActionId { get; private set; }

        public int PreviousActionId { get; private set; }

        public ActorStateChangeReason LastChangeReason { get; private set; } = ActorStateChangeReason.None;

        public bool HasActionRequest => RequestedActionId > 0;

        public bool HasCurrentAction => CurrentActionId > 0;

        public void ChangeState(ActorStateId state, ActorStateChangeReason reason)
        {
            if (state == ActorStateId.None || CurrentState == state)
            {
                return;
            }

            PreviousState = CurrentState;
            CurrentState = state;
            StateFrame = 0;
            StateTime = TimeSpan.Zero;
            LastChangeReason = reason;
        }

        public void Tick(TimeSpan ts)
        {
            StateFrame++;
            StateTime += ts;
        }

        public void RequestAction(int actionId)
        {
            RequestedActionId = actionId > 0 ? actionId : 0;
        }

        public void ClearActionRequest()
        {
            RequestedActionId = 0;
        }

        public void SetCurrentAction(int actionId)
        {
            if (actionId <= 0)
            {
                return;
            }

            PreviousActionId = CurrentActionId;
            CurrentActionId = actionId;
        }

        public void ClearCurrentAction()
        {
            PreviousActionId = CurrentActionId;
            CurrentActionId = 0;
        }
    }
}
