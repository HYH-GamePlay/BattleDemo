using System;
using System.Collections.Generic;
using GameCore.FSM;

namespace Combat.FSM
{
    /// <summary>
    /// 实体战斗状态机 — 包装 GameCore.Fsm，提供转换合法性验证。
    /// </summary>
    public class EntityFsm
    {
        private readonly Fsm<Core.CombatEntity> _fsm;
        private readonly Core.EventBus _bus;

        // 转换表：key=当前状态类型，value=允许转入的状态类型集合
        private static readonly Dictionary<Type, HashSet<Type>> TransitionTable = new()
        {
            [typeof(States.IdleState)]    = new() { typeof(States.AttackState), typeof(States.DefendState), typeof(States.DodgeState), typeof(States.StunnedState), typeof(States.DeadState) },
            [typeof(States.AttackState)]  = new() { typeof(States.IdleState), typeof(States.StunnedState), typeof(States.DeadState) },
            [typeof(States.DefendState)]  = new() { typeof(States.IdleState), typeof(States.AttackState), typeof(States.StunnedState), typeof(States.DeadState) },
            [typeof(States.DodgeState)]   = new() { typeof(States.IdleState), typeof(States.DeadState) },
            [typeof(States.StunnedState)] = new() { typeof(States.IdleState), typeof(States.DeadState) },
            [typeof(States.DeadState)]    = new(),
        };

        public string CurrentStateName => _fsm.CurrentStateName;

        public EntityFsm(Core.CombatEntity owner, Core.EventBus bus)
        {
            _bus = bus;
            _fsm = Fsm<Core.CombatEntity>.Creat("EntityFsm", owner, new List<FsmState<Core.CombatEntity>>
            {
                new States.IdleState(bus),
                new States.AttackState(bus),
                new States.DefendState(bus),
                new States.DodgeState(bus),
                new States.StunnedState(bus),
                new States.DeadState(bus),
            });
            _fsm.Start<States.IdleState>();
        }

        public bool CanTransitionTo<TState>() where TState : FsmState<Core.CombatEntity>
        {
            var current = _fsm.CurrentState?.GetType();
            return current != null
                && TransitionTable.TryGetValue(current, out var allowed)
                && allowed.Contains(typeof(TState));
        }

        public bool TransitionTo<TState>() where TState : FsmState<Core.CombatEntity>
        {
            if (!CanTransitionTo<TState>()) return false;
            _fsm.ChangeState<TState>();
            return true;
        }

        public void Tick(float delta) => _fsm.OnTick(TimeSpan.FromSeconds(delta));

        public void SetData<T>(string name, T value) => _fsm.SetData(name, value);
        public T GetData<T>(string name) => _fsm.GetData<T>(name);
    }
}
