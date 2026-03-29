using System;

namespace GameCore{
    public interface ITickable{
        void OnTick(TimeSpan ts);
    }
    
    public interface ITickComp{
        void Register(ITickable tickable);
        void Unregister(ITickable tickable);
    }

}