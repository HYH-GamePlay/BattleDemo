using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Tools.PoolLinq{
    /// <summary>
    ///     <para>A Collection such as List, HashSet, Dictionary etc can be pooled and reused by using a CollectionPool.</para>
    /// </summary>
    public class StackPool<TItem>{
        internal static readonly ObjectPool<Stack<TItem>> s_Pool = new((Func<Stack<TItem>>)(() => new Stack<TItem>()),
            actionOnRelease: (Action<Stack<TItem>>)(l => l.Clear()));

        public static Stack<TItem> Get(){
            return s_Pool.Get();
        }

        public static PooledObject<Stack<TItem>> Get(out Stack<TItem> value){
            return s_Pool.Get(out value);
        }

        public static void Release(Stack<TItem> toRelease){
            s_Pool.Release(toRelease);
        }
    }

    /// <summary>
    ///     <para>A Collection such as List, HashSet, Dictionary etc can be pooled and reused by using a CollectionPool.</para>
    /// </summary>
    public class QueuePool<TItem>{
        internal static readonly ObjectPool<Queue<TItem>> s_Pool = new((Func<Queue<TItem>>)(() => new Queue<TItem>()),
            actionOnRelease: (Action<Queue<TItem>>)(l => l.Clear()));

        public static Queue<TItem> Get(){
            return s_Pool.Get();
        }

        public static PooledObject<Queue<TItem>> Get(out Queue<TItem> value){
            return s_Pool.Get(out value);
        }

        public static void Release(Queue<TItem> toRelease){
            s_Pool.Release(toRelease);
        }
    }
}