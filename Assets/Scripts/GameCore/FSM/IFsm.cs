using System;
using GameCore.Core.Comp.Tick;

namespace GameCore.FSM{
    public interface IFsm<T>: ITickable where T : class{
        public string Name{ get; }
        public T Owner{ get; }
        public FsmState<T> CurrentState{ get; }
        void ChangeState<TFsmState>() where TFsmState : FsmState<T>;
        void ChangeState<TFsmState>(TFsmState state) where TFsmState : FsmState<T>;
        FsmState<T> GetState(Type state);
        FsmState<T> GetState<TFsmState>() where TFsmState : FsmState<T>;
        void SetData<TValueType>(string name, TValueType value);
        TData GetData<TData>(string name);
    }
}