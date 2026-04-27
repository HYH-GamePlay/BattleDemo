using System;

namespace GameCore.Core.Comp.Tick{
    public interface ITickable{
        void OnTick(TimeSpan ts);
    }
    
    public interface ITickComp{
        void Register(ITickable tickable);
        void Unregister(ITickable tickable);
        
        void Tick(TimeSpan ts);
    }

}