using System;
using System.Collections.Generic;
using Battle.Behavior.Action.Executors;
using Battle.Behavior.State;
using Battle.CombatInfo.Action;
using Battle.CombatInfo.Effect;
using Battle.CombatInfo.State;
using Battle.CombatInfo.Tag;
using Battle.Core;

namespace Battle.Behavior.Action
{
    public sealed class ActionBehavior : BehaviorBase
    {
        private readonly Dictionary<InstructType, Executor> _executors = new Dictionary<InstructType, Executor>();
        private readonly Dictionary<int, ActionData> _actions = new Dictionary<int, ActionData>();
        private readonly Dictionary<int, EffectData> _effects = new Dictionary<int, EffectData>();
        private readonly Dictionary<int, uint> _inputBuffer = new Dictionary<int, uint>();
        private readonly List<int> _expiredInputs = new List<int>();
        private readonly List<ActionRuntimeInfo> _runtimeTickBuffer = new List<ActionRuntimeInfo>();

        private CombatWorld _world;
        private ActionBehaviorInfo _info;
        private InfoHandle _infoHandle = InfoHandle.Invalid;
        private long _nextRuntimeId = 1;

        public override CombatPhase Phase => CombatPhase.Action;

        public InfoHandle InfoHandle => _infoHandle;

        public ActionBehaviorInfo Info => _info;

        public Func<int, bool> CanPlayAction { get; set; }

        protected override void OnAttached(CombatWorld world)
        {
            _world = world;
            _infoHandle = world.AddActorInfo(Owner, new ActionBehaviorInfo());
            _info = world.GetInfo<ActionBehaviorInfo>(_infoHandle);
            RegisterDefaultExecutors();
            BindStateBehavior(world);
        }

        protected override void OnDetached(CombatWorld world)
        {
            CancelAll(world);
            if (_infoHandle.IsValid)
            {
                world.RemoveInfo(_infoHandle);
            }

            _info = null;
            _infoHandle = InfoHandle.Invalid;
            _world = null;
            _executors.Clear();
            _actions.Clear();
            _effects.Clear();
            _inputBuffer.Clear();
        }

        protected override void OnTick(CombatWorld world, in CombatTime time)
        {
            TickInputBuffer();
            TickRuntimes(world, time);
            TryPlayPendingAction();
        }

        public void RegisterExecutor(InstructType type, Executor executor)
        {
            if (type == InstructType.None)
            {
                throw new ArgumentException("Executor type cannot be None.", nameof(type));
            }

            if (executor == null)
            {
                throw new ArgumentNullException(nameof(executor));
            }

            _executors[type] = executor;
        }

        public void RegisterAction(int actionId, ActionData data)
        {
            if (actionId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(actionId), "Action id must be positive.");
            }

            _actions[actionId] = data ?? throw new ArgumentNullException(nameof(data));
        }

        public bool TryGetActionData(int actionId, out ActionData data)
        {
            return _actions.TryGetValue(actionId, out data);
        }

        public void RegisterEffectData(int effectId, EffectData data)
        {
            if (effectId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(effectId), "Effect id must be positive.");
            }

            _effects[effectId] = data ?? throw new ArgumentNullException(nameof(data));
        }

        public bool TryGetEffectData(int effectId, out EffectData data)
        {
            return _effects.TryGetValue(effectId, out data);
        }

        public ActionRuntimeInfo PlayAction(int actionId)
        {
            if (!CanPlay(actionId))
            {
                return null;
            }

            var data = _actions[actionId];
            return Play(data, actionId);
        }

        public ActionRuntimeInfo Play(ActionData data, int actionId = 0)
        {
            if (data == null || _info == null)
            {
                return null;
            }

            var runtime = new ActionRuntimeInfo(_nextRuntimeId++, Owner, actionId, data);
            _info.AddRuntime(runtime);
            NotifyActionStarted(actionId);
            MarkInfoDirty();
            return runtime;
        }

        public bool Cancel(long runtimeId)
        {
            if (_info == null)
            {
                return false;
            }

            for (var i = 0; i < _info.Runtimes.Count; i++)
            {
                var runtime = _info.Runtimes[i];
                if (runtime.RuntimeId == runtimeId)
                {
                    runtime.IsCanceled = true;
                    return true;
                }
            }

            return false;
        }

        public void CancelAll(CombatWorld world)
        {
            if (_info == null)
            {
                return;
            }

            for (var i = _info.Runtimes.Count - 1; i >= 0; i--)
            {
                var runtime = _info.Runtimes[i];
                runtime.IsCanceled = true;
                ExitAllActiveInstructs(world, runtime, default);
                runtime.IsFinished = true;
                _info.RemoveRuntime(runtime);
                NotifyActionFinished(runtime.ActionId);
            }

            MarkInfoDirty();
        }

        public void BufferInput(int input, uint bufferFrames = 8)
        {
            if (input <= 0 || bufferFrames == 0)
            {
                return;
            }

            _inputBuffer[input] = bufferFrames;
        }

        public bool HasBufferedInput(int input)
        {
            return input > 0 && _inputBuffer.ContainsKey(input);
        }

        public bool ConsumeBufferedInput(int input)
        {
            return input > 0 && _inputBuffer.Remove(input);
        }

        public void EvaluateActionLink(ActionRuntimeInfo runtime, ActionLinkData data)
        {
            if (runtime == null || data == null || data.branches == null || data.branches.Count == 0)
            {
                return;
            }

            ActionLinkBranchData best = null;
            for (var i = 0; i < data.branches.Count; i++)
            {
                var branch = data.branches[i];
                if (branch == null || branch.nextAction <= 0)
                {
                    continue;
                }

                if (branch.input > 0 && !HasBufferedInput(branch.input))
                {
                    continue;
                }

                if (!IsBranchConditionSatisfied(runtime, branch))
                {
                    continue;
                }

                if (!CanPlay(branch.nextAction))
                {
                    continue;
                }

                if (best == null || branch.priority > best.priority)
                {
                    best = branch;
                }
            }

            if (best == null)
            {
                return;
            }

            if (data.consumeInput && best.input > 0)
            {
                ConsumeBufferedInput(best.input);
            }

            _info.PendingActionId = best.nextAction;
            if (best.transition == ActionTransitionPolicy.Immediate)
            {
                runtime.IsCanceled = true;
            }

            MarkInfoDirty();
        }

        private bool CanPlay(int actionId)
        {
            return TryGetActionData(actionId, out _) && (CanPlayAction == null || CanPlayAction(actionId));
        }

        private void TickRuntimes(CombatWorld world, in CombatTime time)
        {
            _runtimeTickBuffer.Clear();
            for (var i = 0; i < _info.Runtimes.Count; i++)
            {
                _runtimeTickBuffer.Add(_info.Runtimes[i]);
            }

            for (var i = 0; i < _runtimeTickBuffer.Count; i++)
            {
                var runtime = _runtimeTickBuffer[i];
                if (runtime.IsFinished)
                {
                    continue;
                }

                TickRuntime(world, time, runtime);
            }
        }

        private void TickRuntime(CombatWorld world, in CombatTime time, ActionRuntimeInfo runtime)
        {
            if (runtime.IsCanceled)
            {
                FinishRuntime(world, runtime, time);
                return;
            }

            var data = runtime.Data;
            var length = data.GetLength();
            var frame = runtime.ElapsedFrames;

            for (var i = 0; i < data.instructs.Count; i++)
            {
                var instruct = data.instructs[i];
                if (instruct == null || !instruct.IsValid)
                {
                    continue;
                }

                if (frame >= instruct.begin && frame < instruct.end)
                {
                    ExecuteInstruct(world, time, runtime, instruct, i);
                    if (runtime.IsCanceled)
                    {
                        break;
                    }
                }
            }

            if (runtime.IsCanceled)
            {
                FinishRuntime(world, runtime, time);
                return;
            }

            runtime.ElapsedFrames++;
            ExitEndedInstructs(world, time, runtime);

            if (runtime.ElapsedFrames >= length)
            {
                FinishRuntime(world, runtime, time);
            }
        }

        private void ExecuteInstruct(CombatWorld world, in CombatTime time, ActionRuntimeInfo runtime, Instruct instruct, int index)
        {
            if (!_executors.TryGetValue(instruct.data.id, out var executor))
            {
                return;
            }

            var context = new ActionExecutionContext(world, this, runtime, instruct, index, time);
            if (runtime.MarkInstructActive(index))
            {
                executor.Enter(context, instruct.data);
            }

            executor.Execute(context, instruct.data);
        }

        private void ExitEndedInstructs(CombatWorld world, in CombatTime time, ActionRuntimeInfo runtime)
        {
            if (runtime.ActiveInstructs.Count == 0)
            {
                return;
            }

            var data = runtime.Data;
            var frame = runtime.ElapsedFrames;
            var exited = false;

            for (var i = data.instructs.Count - 1; i >= 0; i--)
            {
                if (!runtime.IsInstructActive(i))
                {
                    continue;
                }

                var instruct = data.instructs[i];
                if (instruct == null || frame < instruct.end)
                {
                    continue;
                }

                ExitInstruct(world, time, runtime, instruct, i);
                exited = true;
            }

            if (exited)
            {
                MarkInfoDirty();
            }
        }

        private void FinishRuntime(CombatWorld world, ActionRuntimeInfo runtime, in CombatTime time)
        {
            ExitAllActiveInstructs(world, runtime, time);
            runtime.IsFinished = true;
            _info.RemoveRuntime(runtime);
            NotifyActionFinished(runtime.ActionId);
            MarkInfoDirty();
        }

        private void ExitAllActiveInstructs(CombatWorld world, ActionRuntimeInfo runtime, in CombatTime time)
        {
            if (runtime.ActiveInstructs.Count == 0)
            {
                return;
            }

            var data = runtime.Data;
            for (var i = data.instructs.Count - 1; i >= 0; i--)
            {
                if (!runtime.IsInstructActive(i))
                {
                    continue;
                }

                var instruct = data.instructs[i];
                if (instruct != null)
                {
                    ExitInstruct(world, time, runtime, instruct, i);
                }
            }

            runtime.ClearActiveInstructs();
        }

        private void ExitInstruct(CombatWorld world, in CombatTime time, ActionRuntimeInfo runtime, Instruct instruct, int index)
        {
            if (instruct.data != null && _executors.TryGetValue(instruct.data.id, out var executor))
            {
                var context = new ActionExecutionContext(world, this, runtime, instruct, index, time);
                executor.Exit(context, instruct.data);
            }

            runtime.MarkInstructInactive(index);
        }

        private void TryPlayPendingAction()
        {
            if (_info == null || !_info.HasPendingAction)
            {
                return;
            }

            var actionId = _info.PendingActionId;
            _info.ClearPendingAction();
            if (CanPlay(actionId))
            {
                PlayAction(actionId);
            }

            MarkInfoDirty();
        }

        private void TickInputBuffer()
        {
            if (_inputBuffer.Count == 0)
            {
                return;
            }

            _expiredInputs.Clear();
            foreach (var pair in _inputBuffer)
            {
                if (pair.Value <= 1)
                {
                    _expiredInputs.Add(pair.Key);
                }
                else
                {
                    _expiredInputs.Add(-pair.Key);
                }
            }

            for (var i = 0; i < _expiredInputs.Count; i++)
            {
                var input = _expiredInputs[i];
                if (input > 0)
                {
                    _inputBuffer.Remove(input);
                }
                else
                {
                    input = -input;
                    _inputBuffer[input] = _inputBuffer[input] - 1;
                }
            }
        }

        private void RegisterDefaultExecutors()
        {
            _executors.Clear();
            RegisterExecutor(InstructType.Animation, new EmptyExecutor<AnimationData>());
            RegisterExecutor(InstructType.AddTag, new AddTagExecutor());
            RegisterExecutor(InstructType.RemoveTag, new RemoveTagExecutor());
            RegisterExecutor(InstructType.Collision, new CollisionExecutor());
            RegisterExecutor(InstructType.ApplyEffect, new ApplyEffectExecutor());
            RegisterExecutor(InstructType.Motion, new EmptyExecutor<MotionData>());
            RegisterExecutor(InstructType.CreateBullet, new EmptyExecutor<CreateBulletData>());
            RegisterExecutor(InstructType.Audio, new EmptyExecutor<AudioData>());
            RegisterExecutor(InstructType.Camera, new EmptyExecutor<CameraData>());
            RegisterExecutor(InstructType.Vfx, new EmptyExecutor<VfxData>());
            RegisterExecutor(InstructType.ActionLink, new ActionLinkExecutor());
        }

        private bool IsBranchConditionSatisfied(ActionRuntimeInfo runtime, ActionLinkBranchData branch)
        {
            if (branch.beginFrame > 0 && runtime.ElapsedFrames < branch.beginFrame)
            {
                return false;
            }

            if (branch.endFrame > 0 && runtime.ElapsedFrames >= branch.endFrame)
            {
                return false;
            }

            return IsStateConditionSatisfied(branch) && AreTagConditionsSatisfied(branch);
        }

        private bool IsStateConditionSatisfied(ActionLinkBranchData branch)
        {
            if (branch.requiredState == ActorStateId.None && branch.blockedState == ActorStateId.None)
            {
                return true;
            }

            if (_world == null || !_world.TryGetActorInfo(Owner, out InfoHandle _, out ActorStateInfo stateInfo))
            {
                return branch.requiredState == ActorStateId.None;
            }

            if (branch.requiredState != ActorStateId.None && stateInfo.CurrentState != branch.requiredState)
            {
                return false;
            }

            return branch.blockedState == ActorStateId.None || stateInfo.CurrentState != branch.blockedState;
        }

        private bool AreTagConditionsSatisfied(ActionLinkBranchData branch)
        {
            var hasRequiredTags = branch.requiredTags != null && branch.requiredTags.Count > 0;
            var hasBlockedTags = branch.blockedTags != null && branch.blockedTags.Count > 0;
            if (!hasRequiredTags && !hasBlockedTags)
            {
                return true;
            }

            if (_world == null || !_world.TryGetActorInfo(Owner, out InfoHandle _, out TagContainerInfo tagInfo))
            {
                return !hasRequiredTags;
            }

            if (hasRequiredTags)
            {
                for (var i = 0; i < branch.requiredTags.Count; i++)
                {
                    var tag = new TagId(branch.requiredTags[i]);
                    if (tag.IsValid && !tagInfo.Tags.Contains(tag))
                    {
                        return false;
                    }
                }
            }

            if (hasBlockedTags)
            {
                for (var i = 0; i < branch.blockedTags.Count; i++)
                {
                    var tag = new TagId(branch.blockedTags[i]);
                    if (tag.IsValid && tagInfo.Tags.Contains(tag))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void BindStateBehavior(CombatWorld world)
        {
            if (world.TryGetBehavior(Owner, out ActorStateBehavior stateBehavior) && CanPlayAction == null)
            {
                CanPlayAction = stateBehavior.CanPlayAction;
            }
        }

        private void NotifyActionStarted(int actionId)
        {
            if (actionId > 0 && _world != null && _world.TryGetBehavior(Owner, out ActorStateBehavior stateBehavior))
            {
                stateBehavior.NotifyActionStarted(actionId);
            }
        }

        private void NotifyActionFinished(int actionId)
        {
            if (actionId > 0 && _world != null && _world.TryGetBehavior(Owner, out ActorStateBehavior stateBehavior))
            {
                stateBehavior.NotifyActionFinished(actionId);
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
