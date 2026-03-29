using UnityEngine;

namespace Tools.Singleton{
    public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>{
        private static T _instance;

        public static T Instance{
            get{
                if (_instance == null){
                    _instance = FindObjectOfType<T>();

                    if (_instance == null){
                        var singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake(){
            if (_instance == null){
                _instance = (T)this;
                DontDestroyOnLoad(gameObject);
            }
            else{
                Destroy(gameObject);
            }
        }
    }
}