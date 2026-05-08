using System;
using System.Collections.Generic;
using Battle.Behavior;
using Battle.CombatInfo;
using Tools.Log;

namespace Battle.Core
{
    public sealed class CombatWorld
    {
        private readonly CombatInfoStore _infos = new CombatInfoStore();
        private readonly HashSet<ActorId> _actors = new HashSet<ActorId>();
        private readonly Dictionary<BehaviorHandle, IBehavior> _behaviors = new Dictionary<BehaviorHandle, IBehavior>();
        private readonly Dictionary<ActorId, List<BehaviorHandle>> _actorBehaviors = new Dictionary<ActorId, List<BehaviorHandle>>();
        private readonly Dictionary<ActorId, Dictionary<Type, InfoHandle>> _actorInfos = new Dictionary<ActorId, Dictionary<Type, InfoHandle>>();
        private readonly List<IBehavior> _tickBuffer = new List<IBehavior>(64);

        private int _nextActorId = 1;
        private int _nextBehaviorHandle = 1;
        private TimeSpan _elapsedTime;

        public long Frame { get; private set; }

        public ActorId CreateActor()
        {
            var actorId = new ActorId(_nextActorId++);
            _actors.Add(actorId);
            _actorBehaviors.Add(actorId, new List<BehaviorHandle>());
            _actorInfos.Add(actorId, new Dictionary<Type, InfoHandle>());
            return actorId;
        }

        public bool DestroyActor(ActorId actorId)
        {
            if (!_actorBehaviors.TryGetValue(actorId, out var handles))
            {
                return false;
            }

            for (var i = handles.Count - 1; i >= 0; i--)
            {
                DetachBehavior(handles[i]);
            }

            if (_actorInfos.TryGetValue(actorId, out var infoHandles))
            {
                foreach (var handle in infoHandles.Values)
                {
                    RemoveInfo(handle);
                }
            }

            _actorInfos.Remove(actorId);
            _actorBehaviors.Remove(actorId);
            return _actors.Remove(actorId);
        }

        public bool HasActor(ActorId actorId)
        {
            return _actors.Contains(actorId);
        }

        public IReadOnlyCollection<ActorId> Actors => _actors;

        public InfoHandle AddInfo<T>(T info) where T : class, IBehaviorInfo
        {
            return _infos.AddInfo(info);
        }

        public InfoHandle AddActorInfo<T>(ActorId actorId, T info) where T : class, IBehaviorInfo
        {
            if (!_actorInfos.TryGetValue(actorId, out var infos))
            {
                throw new InvalidOperationException($"Actor does not exist: {actorId}");
            }

            var type = typeof(T);
            if (infos.ContainsKey(type))
            {
                throw new InvalidOperationException($"Actor info already exists. Actor: {actorId}, Info: {type.Name}");
            }

            var handle = AddInfo(info);
            infos.Add(type, handle);
            return handle;
        }

        public InfoHandle GetOrAddActorInfo<T>(ActorId actorId) where T : class, IBehaviorInfo, new()
        {
            if (TryGetActorInfo(actorId, out InfoHandle handle, out T _))
            {
                return handle;
            }

            return AddActorInfo(actorId, new T());
        }

        public bool TryGetActorInfo<T>(ActorId actorId, out InfoHandle handle, out T info) where T : class, IBehaviorInfo
        {
            handle = InfoHandle.Invalid;
            info = null;
            if (!_actorInfos.TryGetValue(actorId, out var infos))
            {
                return false;
            }

            if (!infos.TryGetValue(typeof(T), out handle))
            {
                return false;
            }

            return TryGetInfo(handle, out info);
        }

        public bool RemoveInfo(InfoHandle handle)
        {
            return _infos.RemoveInfo(handle);
        }

        public bool RemoveInfo(IBehaviorInfo info)
        {
            return _infos.RemoveInfo(info);
        }

        public bool HasInfo(InfoHandle handle)
        {
            return _infos.HasInfo(handle);
        }

        public bool HasInfo(IBehaviorInfo info)
        {
            return _infos.HasInfo(info);
        }

        public InfoHandle GetInfoHandle(IBehaviorInfo info)
        {
            return _infos.GetHandle(info);
        }

        public T GetInfo<T>(InfoHandle handle) where T : class, IBehaviorInfo
        {
            return _infos.GetInfo<T>(handle);
        }

        public bool TryGetInfo<T>(InfoHandle handle, out T info) where T : class, IBehaviorInfo
        {
            return _infos.TryGetInfo(handle, out info);
        }

        public void MarkInfoDirty(InfoHandle handle)
        {
            _infos.MarkDirty(handle);
        }

        public void MarkInfoDirty(IBehaviorInfo info)
        {
            _infos.MarkDirty(info);
        }

        public uint GetInfoVersion(InfoHandle handle)
        {
            return _infos.GetVersion(handle);
        }

        public BehaviorHandle AttachBehavior(ActorId owner, IBehavior behavior)
        {
            if (behavior == null)
            {
                throw new ArgumentNullException(nameof(behavior));
            }

            if (!_actorBehaviors.TryGetValue(owner, out var handles))
            {
                throw new InvalidOperationException($"Actor does not exist: {owner}");
            }

            var handle = new BehaviorHandle(_nextBehaviorHandle++);
            _behaviors.Add(handle, behavior);
            handles.Add(handle);
            behavior.OnAttach(this, owner, handle);
            return handle;
        }

        public bool DetachBehavior(BehaviorHandle handle)
        {
            if (!_behaviors.TryGetValue(handle, out var behavior))
            {
                return false;
            }

            var owner = behavior.Owner;
            behavior.OnDetach(this);
            _behaviors.Remove(handle);

            if (_actorBehaviors.TryGetValue(owner, out var handles))
            {
                handles.Remove(handle);
            }

            return true;
        }

        public bool TryGetBehavior(BehaviorHandle handle, out IBehavior behavior)
        {
            return _behaviors.TryGetValue(handle, out behavior);
        }

        public bool TryGetBehavior<T>(ActorId actorId, out T behavior) where T : class, IBehavior
        {
            behavior = null;
            if (!_actorBehaviors.TryGetValue(actorId, out var handles))
            {
                return false;
            }

            for (var i = 0; i < handles.Count; i++)
            {
                if (_behaviors.TryGetValue(handles[i], out var value) && value is T typed)
                {
                    behavior = typed;
                    return true;
                }
            }

            return false;
        }

        public IReadOnlyList<BehaviorHandle> GetBehaviorHandles(ActorId actorId)
        {
            if (_actorBehaviors.TryGetValue(actorId, out var handles))
            {
                return handles;
            }

            throw new InvalidOperationException($"Actor does not exist: {actorId}");
        }

        public void Tick(TimeSpan ts)
        {
            if (ts.TotalMilliseconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(ts), "Delta time cannot be negative.");
            }

            Frame++;
            _elapsedTime += ts;
            var time = new CombatTime(Frame, ts, _elapsedTime);

            TickPhase(CombatPhase.PreUpdate, time);
            TickPhase(CombatPhase.Input, time);
            TickPhase(CombatPhase.Action, time);
            TickPhase(CombatPhase.Movement, time);
            TickPhase(CombatPhase.Hit, time);
            TickPhase(CombatPhase.Damage, time);
            TickPhase(CombatPhase.State, time);
            TickPhase(CombatPhase.Presentation, time);
            TickPhase(CombatPhase.PostUpdate, time);
        }

        private void TickPhase(CombatPhase phase, in CombatTime time)
        {
            _tickBuffer.Clear();
            foreach (var behavior in _behaviors.Values)
            {
                if (behavior.IsActive && behavior.Phase == phase)
                {
                    _tickBuffer.Add(behavior);
                }
            }

            for (var i = 0; i < _tickBuffer.Count; i++)
            {
                var behavior = _tickBuffer[i];
                if (behavior.IsActive)
                {
                    behavior.Tick(this, time);
                }
            }
        }
    }
}
