using System;

namespace GameCore{
    public interface IEventComp : IComp{
        void AddListener<T>(int eventType, Action<T> handler);
        void RemoveListener<T>(int eventType, Action<T> handler);
        void Broadcast<T>(int eventType, T data);
        
        void AddListener(int eventType, Action handler);
        void RemoveListener(int eventType, Action handler);
        void Broadcast(int eventType);
        
        
    }
}