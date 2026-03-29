using System;

namespace GameCore.FSM{
    public abstract class FsmBase{
        public abstract int FsmStateCount{ get; }

        public abstract bool IsRunning{ get; }

        public abstract bool IsDestroyed{ get; }

        public abstract string CurrentStateName{ get; }

        public abstract float CurrentStateTime{ get; }

        public virtual void OnTick(TimeSpan ts){ }
    }
}