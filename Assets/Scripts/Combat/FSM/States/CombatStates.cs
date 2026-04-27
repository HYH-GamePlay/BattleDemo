using System;
using GameCore.FSM;

namespace Combat.FSM.States
{
    public abstract class CombatStateBase : FsmState<Core.CombatEntity>
    {
        protected readonly Core.EventBus Bus;

        protected CombatStateBase(Core.EventBus bus) => Bus = bus;

        protected void PublishStateChanged(Core.CombatEntity owner, string oldState, string newState)
            => Bus.Publish(new Core.StateChangedEvent { EntityId = owner.EntityId, OldState = oldState, NewState = newState });
    }

    public class IdleState : CombatStateBase
    {
        public IdleState(Core.EventBus bus) : base(bus) { name = nameof(IdleState); }

        public override void OnEnter(IFsm<Core.CombatEntity> fsm)
            => PublishStateChanged(fsm.Owner, fsm.GetData<string>("prevState") ?? "", name);
    }

    public class AttackState : CombatStateBase
    {
        private float _duration;
        private float _elapsed;

        public AttackState(Core.EventBus bus) : base(bus) { name = nameof(AttackState); }

        public override void OnEnter(IFsm<Core.CombatEntity> fsm)
        {
            _elapsed = 0f;
            _duration = fsm.GetData<float>("attackDuration");
            if (_duration <= 0f) _duration = 0.5f;
            fsm.SetData("prevState", name);
            PublishStateChanged(fsm.Owner, "", name);
        }

        public override void OnTick(TimeSpan ts)
        {
            _elapsed += (float)ts.TotalSeconds;
            if (_elapsed >= _duration) ChangeState<IdleState>();
        }
    }

    public class DefendState : CombatStateBase
    {
        private float _holdTime;

        public DefendState(Core.EventBus bus) : base(bus) { name = nameof(DefendState); }

        public override void OnEnter(IFsm<Core.CombatEntity> fsm)
        {
            _holdTime = 0f;
            fsm.SetData("prevState", name);
            PublishStateChanged(fsm.Owner, "", name);
        }

        public override void OnTick(TimeSpan ts)
        {
            _holdTime += (float)ts.TotalSeconds;
            fsm.SetData("defendHoldTime", _holdTime);
        }

        public override void OnExit() => fsm.SetData("defendHoldTime", 0f);
    }

    public class DodgeState : CombatStateBase
    {
        private float _duration;
        private float _elapsed;

        public DodgeState(Core.EventBus bus) : base(bus) { name = nameof(DodgeState); }

        public override void OnEnter(IFsm<Core.CombatEntity> fsm)
        {
            _elapsed = 0f;
            _duration = fsm.GetData<float>("dodgeDuration");
            if (_duration <= 0f) _duration = 0.3f;
            fsm.SetData("prevState", name);
            PublishStateChanged(fsm.Owner, "", name);
            Bus.Publish(new Core.DodgeEvent { EntityId = fsm.Owner.EntityId });
        }

        public override void OnTick(TimeSpan ts)
        {
            _elapsed += (float)ts.TotalSeconds;
            if (_elapsed >= _duration) ChangeState<IdleState>();
        }
    }

    public class StunnedState : CombatStateBase
    {
        private float _duration;
        private float _elapsed;

        public StunnedState(Core.EventBus bus) : base(bus) { name = nameof(StunnedState); }

        public override void OnEnter(IFsm<Core.CombatEntity> fsm)
        {
            _elapsed = 0f;
            _duration = fsm.GetData<float>("stunDuration");
            if (_duration <= 0f) _duration = 1f;
            fsm.SetData("prevState", name);
            PublishStateChanged(fsm.Owner, "", name);
        }

        public override void OnTick(TimeSpan ts)
        {
            _elapsed += (float)ts.TotalSeconds;
            if (_elapsed >= _duration) ChangeState<IdleState>();
        }
    }

    public class DeadState : CombatStateBase
    {
        public DeadState(Core.EventBus bus) : base(bus) { name = nameof(DeadState); }

        public override void OnEnter(IFsm<Core.CombatEntity> fsm)
            => PublishStateChanged(fsm.Owner, "", name);
    }
}
