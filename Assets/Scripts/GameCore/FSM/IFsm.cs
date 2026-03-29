using System;

namespace GameCore.FSM{
    public interface IFsm<T> where T : class{
        public string Name{ get; }
        public T Owner{ get; }
        public FsmState<T> CurrentState{ get; }

        void OnTick(TimeSpan ts);
        void ChangeState<TFsmState>() where TFsmState : FsmState<T>;
        FsmState<T> GetState(Type state);
        FsmState<T> GetState<TFsmState>() where TFsmState : FsmState<T>;
    }
}