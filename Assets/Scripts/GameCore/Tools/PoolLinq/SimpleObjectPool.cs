using UnityEngine.Pool;

namespace Tools.PoolLinq{
    /// 简单对象池，对象存在默认构造函数，并且在Release时保证数据都是清空的，或者在Get的时候数据都是被覆盖的。
    public class SimpleObjectPool<T> where T : class, new(){
        internal static readonly ObjectPool<T> s_Pool = new(() => new T());

        public static T Get(){
            return s_Pool.Get();
        }

        public static PooledObject<T> Get(out T value){
            return s_Pool.Get(out value);
        }

        public static void Release(T toRelease){
            s_Pool.Release(toRelease);
        }
    }
}