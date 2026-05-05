using System;
using System.Collections.Generic;
using Battle.Ability;
using Battle.CombatInfo;

namespace Battle.Core
{
    public sealed class CombatWorld
    {
        private readonly CombatInfoStore _infos = new CombatInfoStore();
        private readonly HashSet<ActorId> _actors = new HashSet<ActorId>();
        private readonly Dictionary<AbilityHandle, IAbility> _abilities = new Dictionary<AbilityHandle, IAbility>();
        private readonly Dictionary<ActorId, List<AbilityHandle>> _actorAbilities = new Dictionary<ActorId, List<AbilityHandle>>();
        private readonly List<IAbility> _tickBuffer = new List<IAbility>(64);

        private int _nextActorId = 1;
        private int _nextAbilityHandle = 1;
        private float _elapsedTime;

        public long Frame { get; private set; }

        public ActorId CreateActor()
        {
            var actorId = new ActorId(_nextActorId++);
            _actors.Add(actorId);
            _actorAbilities.Add(actorId, new List<AbilityHandle>());
            return actorId;
        }

        public bool DestroyActor(ActorId actorId)
        {
            if (!_actorAbilities.TryGetValue(actorId, out var handles))
            {
                return false;
            }

            for (var i = handles.Count - 1; i >= 0; i--)
            {
                DetachAbility(handles[i]);
            }

            _actorAbilities.Remove(actorId);
            return _actors.Remove(actorId);
        }

        public bool HasActor(ActorId actorId)
        {
            return _actors.Contains(actorId);
        }

        public IReadOnlyCollection<ActorId> Actors => _actors;

        public InfoHandle AddInfo<T>(T info) where T : class, ICombatInfo
        {
            return _infos.AddInfo(info);
        }

        public bool RemoveInfo(InfoHandle handle)
        {
            return _infos.RemoveInfo(handle);
        }

        public bool RemoveInfo(ICombatInfo info)
        {
            return _infos.RemoveInfo(info);
        }

        public bool HasInfo(InfoHandle handle)
        {
            return _infos.HasInfo(handle);
        }

        public bool HasInfo(ICombatInfo info)
        {
            return _infos.HasInfo(info);
        }

        public InfoHandle GetInfoHandle(ICombatInfo info)
        {
            return _infos.GetHandle(info);
        }

        public T GetInfo<T>(InfoHandle handle) where T : class, ICombatInfo
        {
            return _infos.GetInfo<T>(handle);
        }

        public bool TryGetInfo<T>(InfoHandle handle, out T info) where T : class, ICombatInfo
        {
            return _infos.TryGetInfo(handle, out info);
        }

        public void MarkInfoDirty(InfoHandle handle)
        {
            _infos.MarkDirty(handle);
        }

        public void MarkInfoDirty(ICombatInfo info)
        {
            _infos.MarkDirty(info);
        }

        public uint GetInfoVersion(InfoHandle handle)
        {
            return _infos.GetVersion(handle);
        }

        public AbilityHandle AttachAbility(ActorId owner, IAbility ability)
        {
            if (ability == null)
            {
                throw new ArgumentNullException(nameof(ability));
            }

            if (!_actorAbilities.TryGetValue(owner, out var handles))
            {
                throw new InvalidOperationException($"Actor does not exist: {owner}");
            }

            var handle = new AbilityHandle(_nextAbilityHandle++);
            _abilities.Add(handle, ability);
            handles.Add(handle);
            ability.OnAttach(this, owner, handle);
            return handle;
        }

        public bool DetachAbility(AbilityHandle handle)
        {
            if (!_abilities.TryGetValue(handle, out var ability))
            {
                return false;
            }

            var owner = ability.Owner;
            ability.OnDetach(this);
            _abilities.Remove(handle);

            if (_actorAbilities.TryGetValue(owner, out var handles))
            {
                handles.Remove(handle);
            }

            return true;
        }

        public bool TryGetAbility(AbilityHandle handle, out IAbility ability)
        {
            return _abilities.TryGetValue(handle, out ability);
        }

        public IReadOnlyList<AbilityHandle> GetAbilityHandles(ActorId actorId)
        {
            if (_actorAbilities.TryGetValue(actorId, out var handles))
            {
                return handles;
            }

            throw new InvalidOperationException($"Actor does not exist: {actorId}");
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time cannot be negative.");
            }

            Frame++;
            _elapsedTime += deltaTime;
            var time = new CombatTime(Frame, deltaTime, _elapsedTime);

            TickPhase(CombatPhase.PreUpdate, time);
            TickPhase(CombatPhase.Input, time);
            TickPhase(CombatPhase.Ability, time);
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
            foreach (var ability in _abilities.Values)
            {
                if (ability.IsActive && ability.Phase == phase)
                {
                    _tickBuffer.Add(ability);
                }
            }

            for (var i = 0; i < _tickBuffer.Count; i++)
            {
                var ability = _tickBuffer[i];
                if (ability.IsActive)
                {
                    ability.Tick(this, time);
                }
            }
        }
    }
}
