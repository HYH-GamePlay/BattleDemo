using System;
using Object = UnityEngine.Object;

namespace GameCore{
    public class ResComp : IResComp{
        public T Load<T>(string path) where T : Object{
            throw new NotImplementedException();
        }

        public void LoadAsync<T>(string path, Action<T> callback) where T : Object{
            throw new NotImplementedException();
        }

        public void Release(string path){
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