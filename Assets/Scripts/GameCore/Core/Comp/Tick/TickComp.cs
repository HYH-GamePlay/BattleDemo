using System;
using System.Collections.Generic;

namespace GameCore.Core.Comp.Tick{
    public class TickComp : ITickComp{
        private List<ITickable> _tickQueue = new();
        
        public void Register(ITickable tickable){
            _tickQueue.Add(tickable);
        }

        public void Unregister(ITickable tickable){
            _tickQueue.Remove(tickable);
        }
        
        public void Tick(TimeSpan ts){
            foreach (var tickable in _tickQueue){
                tickable.OnTick(ts);
            }
        }
    }
}