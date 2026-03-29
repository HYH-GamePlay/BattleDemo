using Tools.Log;
using UnityEngine;

namespace Tools.Singleton{
    public abstract class DontDestroyMonoSingleton<T> : MonoBehaviour where T : Component{
        private static T _instance;

        public static T Instance{
            get{
                if (_instance != null)
                    return _instance;

                var instanceArr = FindObjectsByType<T>(FindObjectsSortMode.None);
                if (instanceArr.Length != 1)
                    HLog.Log("单例不允许存在多个实例");

                _instance = instanceArr[0];
                if (_instance != null)
                    return _instance;

                var obj = new GameObject{
                    name = typeof(T).Name
                };
                _instance = obj.AddComponent<T>();

                return _instance;
            }
        }

        protected virtual void Awake(){
            if (!Application.isPlaying) return;


            if (_instance == null){
                _instance = this as T;
                DontDestroyOnLoad(transform.gameObject);
            }
            else{
                if (this != _instance) Destroy(gameObject);
            }
        }
    }
}