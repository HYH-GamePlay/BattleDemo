using System;
using Object = UnityEngine.Object;

namespace GameCore{
    public interface IResComp : IComp{
        T Load<T>(string path) where T : Object;
        void LoadAsync<T>(string path, Action<T> callback) where T : Object;
        void Release(string path);
    }
}