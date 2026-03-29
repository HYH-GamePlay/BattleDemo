using System;

namespace GameCore{
    public class EventComp : IEventComp{
        public void AddListener<T>(int eventType, Action<T> handler){
            throw new NotImplementedException();
        }

        public void RemoveListener<T>(int eventType, Action<T> handler){
            throw new NotImplementedException();
        }

        public void Broadcast<T>(int eventType, T data){
            throw new NotImplementedException();
        }

        public void AddListener(int eventType, Action handler){
            throw new NotImplementedException();
        }

        public void RemoveListener(int eventType, Action handler){
            throw new NotImplementedException();
        }

        public void Broadcast(int eventType){
            throw new NotImplementedException();
        }

        public void Init(){
            throw new NotImplementedException();
        }

        public void UnInit(){
            throw new NotImplementedException();
        }
    }
}