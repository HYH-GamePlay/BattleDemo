using System;

namespace GameCore.FSM{
    public interface IFsmState<T> where T : class{
        public void OnEnter(IFsm<T> fsm);
        public void OnTick(TimeSpan ts);
        public void OnExit();
        public void OnDestroy();
    }
}