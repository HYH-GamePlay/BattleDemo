using System;

namespace GameCore.FSM{
    public abstract class FsmState<T> where T : class{
        public string Name{ get; protected set; } = string.Empty;

        public virtual void OnInit(IFsm<T> fsm){ }

        public virtual void OnEnter(IFsm<T> fsm){ }

        public virtual void OnTick(TimeSpan ts){ }

        public virtual void OnExit(){ }

        public virtual void OnDestroy(){ }

        public void ChangeState<TState>(IFsm<T> fsm) where TState : FsmState<T>{
            var fsmImplement = (Fsm<T>)fsm;
            if (fsmImplement == null){
                throw new Exception("fsmImplement is null");
            }

            fsmImplement.ChangeState<TState>();
        }
    }
}