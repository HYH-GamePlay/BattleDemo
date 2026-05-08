using System.Collections.Generic;
using Battle.Behavior.Action;
using Battle.CombatInfo.Action;
using Battle.CombatInfo.State;
using Battle.Core;

namespace Battle.Behavior.State
{
    public sealed class ActorStateBehavior : BehaviorBase
    {
        private readonly Dictionary<int, HashSet<ActorStateId>> _actionAllowedStates =
            new Dictionary<int, HashSet<ActorStateId>>();

        private CombatWorld _world;
        private ActorStateInfo _info;
        private InfoHandle _infoHandle = InfoHandle.Invalid;

        public override CombatPhase Phase => CombatPhase.State;

        public InfoHandle InfoHandle => _infoHandle;

        public ActorStateInfo Info => _info;

        protected override void OnAttached(CombatWorld world)
        {
            _world = world;
            _infoHandle = world.GetOrAddActorInfo<ActorStateInfo>(Owner);
            _info = world.GetInfo<ActorStateInfo>(_infoHandle);

            if (_info.CurrentState == ActorStateId.None)
            {
                _info.ChangeState(ActorStateId.Locomotion, ActorStateChangeReason.Initialize);
                MarkInfoDirty();
            }

            BindActionBehavior(world);
        }

        protected override void OnDetached(CombatWorld world)
        {
            if (_infoHandle.IsValid)
            {
                world.RemoveInfo(_infoHandle);
            }

            _actionAllowedStates.Clear();
            _info = null;
            _infoHandle = InfoHandle.Invalid;
            _world = null;
        }

        protected override void OnTick(CombatWorld world, in CombatTime time)
        {
            _info.Tick(time.DeltaTime);

            if (_info.HasActionRequest)
            {
                TryStartRequestedAction(world);
            }

            if (_info.CurrentState == ActorStateId.Attack && IsActionIdle(world))
            {
                _info.ClearCurrentAction();
                ChangeState(ActorStateId.Locomotion, ActorStateChangeReason.ActionFinished);
            }

            MarkInfoDirty();
        }

        public void RegisterActionStates(int actionId, params ActorStateId[] allowedStates)
        {
            if (actionId <= 0)
            {
                return;
            }

            if (!_actionAllowedStates.TryGetValue(actionId, out var states))
            {
                states = new HashSet<ActorStateId>();
                _actionAllowedStates.Add(actionId, states);
            }

            states.Clear();
            if (allowedStates == null)
            {
                return;
            }

            for (var i = 0; i < allowedStates.Length; i++)
            {
                if (allowedStates[i] != ActorStateId.None)
                {
                    states.Add(allowedStates[i]);
                }
            }
        }

        public bool RequestAction(int actionId)
        {
            if (actionId <= 0 || !CanPlayAction(actionId))
            {
                return false;
            }

            _info.RequestAction(actionId);
            MarkInfoDirty();
            return TryStartRequestedAction(_world);
        }

        public bool CanPlayAction(int actionId)
        {
            if (actionId <= 0 || _info == null)
            {
                return false;
            }

            if (_actionAllowedStates.TryGetValue(actionId, out var states) && states.Count > 0)
            {
                return states.Contains(_info.CurrentState);
            }

            return CanPlayActionByDefault(_info.CurrentState);
        }

        public bool ChangeState(ActorStateId state, ActorStateChangeReason reason)
        {
            if (_info == null || state == ActorStateId.None || _info.CurrentState == state)
            {
                return false;
            }

            _info.ChangeState(state, reason);
            MarkInfoDirty();
            return true;
        }

        public void NotifyActionStarted(int actionId)
        {
            if (_info == null || actionId <= 0)
            {
                return;
            }

            if (_info.CurrentState != ActorStateId.Attack && _info.CurrentState != ActorStateId.Dead)
            {
                _info.ChangeState(ActorStateId.Attack, ActorStateChangeReason.ActionStarted);
            }

            _info.SetCurrentAction(actionId);
            _info.ClearActionRequest();
            MarkInfoDirty();
        }

        public void NotifyActionFinished(int actionId)
        {
            if (_info == null || actionId <= 0)
            {
                return;
            }

            if (_info.CurrentActionId == actionId)
            {
                _info.ClearCurrentAction();
                MarkInfoDirty();
            }
        }

        public bool InterruptTo(ActorStateId state, ActorStateChangeReason reason)
        {
            if (!ChangeState(state, reason))
            {
                return false;
            }

            if (_world != null && _world.TryGetBehavior(Owner, out ActionBehavior actionBehavior))
            {
                actionBehavior.CancelAll(_world);
            }

            _info.ClearActionRequest();
            _info.ClearCurrentAction();
            MarkInfoDirty();
            return true;
        }

        private static bool CanPlayActionByDefault(ActorStateId state)
        {
            return state == ActorStateId.Locomotion ||
                   state == ActorStateId.Attack ||
                   state == ActorStateId.Airborne;
        }

        private bool TryStartRequestedAction(CombatWorld world)
        {
            if (world == null || _info == null || !_info.HasActionRequest)
            {
                return false;
            }

            var actionId = _info.RequestedActionId;
            if (!CanPlayAction(actionId))
            {
                _info.ClearActionRequest();
                MarkInfoDirty();
                return false;
            }

            if (!world.TryGetBehavior(Owner, out ActionBehavior actionBehavior))
            {
                return false;
            }

            var runtime = actionBehavior.PlayAction(actionId);
            if (runtime == null)
            {
                _info.ClearActionRequest();
                MarkInfoDirty();
                return false;
            }

            return true;
        }

        private bool IsActionIdle(CombatWorld world)
        {
            if (_info.HasActionRequest)
            {
                return false;
            }

            return !world.TryGetActorInfo(Owner, out InfoHandle _, out ActionBehaviorInfo actionInfo) ||
                   (!actionInfo.HasRunningRuntimes && !actionInfo.HasPendingAction);
        }

        private void BindActionBehavior(CombatWorld world)
        {
            if (world.TryGetBehavior(Owner, out ActionBehavior actionBehavior) &&
                actionBehavior.CanPlayAction == null)
            {
                actionBehavior.CanPlayAction = CanPlayAction;
            }
        }

        private void MarkInfoDirty()
        {
            if (_world != null && _infoHandle.IsValid)
            {
                _world.MarkInfoDirty(_infoHandle);
            }
        }
    }
}
