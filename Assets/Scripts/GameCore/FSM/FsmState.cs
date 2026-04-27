using System;

namespace GameCore.FSM{
    public abstract class FsmState<T> : IFsmState<T> where T : class{
        public string name{ get; protected set; } = string.Empty;
        protected IFsm<T> fsm{ get; private set; }

        public virtual void OnInit(IFsm<T> fsm){
            this.fsm = (Fsm<T>)fsm;
        }

        public virtual void OnEnter(IFsm<T> fsm){ }

        public virtual void OnTick(TimeSpan ts){ }

        public virtual void OnExit(){ }

        public virtual void OnDestroy(){ }

        public void ChangeState<TState>() where TState : FsmState<T>{
            fsm.ChangeState<TState>();
        }

        public void ChangeState<TState>(TState state) where TState : FsmState<T>{
            fsm.ChangeState(state);
        }
    }
}