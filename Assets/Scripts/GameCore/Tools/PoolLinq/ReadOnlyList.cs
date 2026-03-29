using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace Tools.PoolLinq{
    // public class Demo
    // {
    //     public void Test()
    //     {
    //         List<int> a = new List<int>();
    //         ReadOnlyList<int> read = new ReadOnlyList<int>(a);
    //         // equals
    //         if (read == default)
    //         {
    //             return;
    //         }
    //
    //         // foreach
    //         foreach (var i in read)
    //         {
    //             Debug.Log(i);
    //         }
    //
    //         // for
    //         for (var i = 0; i < read.Count; i++)
    //         {
    //             Debug.Log(read[i]);
    //         }
    //
    //         // GC， 虽然实现了 IEnumerable 接口，但linq尽量还是使用PoolLinq的无GC方案
    //         Debug.Log(read.FirstOrDefault(x => x != 0));
    //         // No GC， 转池化列表，linq避免GC
    //         Debug.Log(read.ToPoolList().DelayReleaseList().NoGCFirstOrDefault(x => x != 0));
    //     }
    // }

    public readonly struct ReadOnlyList<T> : IReadOnlyList<T>, IEquatable<ReadOnlyList<T>>{
        private readonly List<T> _list;

        public int Count => _list?.Count ?? 0;
        public T this[int index] => _list == null ? default : _list[index];

        public ReadOnlyList(List<T> list){
            _list = list;
        }

        public List<T>.Enumerator GetEnumerator(){
            return _list?.GetEnumerator() ?? default;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator(){
            return _list?.GetEnumerator() ?? new List<T>.Enumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return _list?.GetEnumerator() ?? new List<T>.Enumerator();
        }

        public bool IsValid(){
            return _list != null;
        }

        public bool Contains(T item){
            return _list?.Contains(item) ?? false;
        }

        public int IndexOf(T item){
            if (_list == null)
                return -1;
            return _list.IndexOf(item);
        }

        public int FindIndex(Predicate<T> match){
            if (_list == null)
                return -1;
            return _list.FindIndex(match);
        }

        public static bool operator ==(ReadOnlyList<T> lhs, ReadOnlyList<T> rhs){
            return lhs._list == rhs._list;
        }

        public static bool operator !=(ReadOnlyList<T> lhs, ReadOnlyList<T> rhs){
            return lhs._list != rhs._list;
        }

        public bool Equals(ReadOnlyList<T> other){
            return Equals(_list, other._list);
        }

        public override bool Equals(object obj){
            return obj is ReadOnlyList<T> other && Equals(other);
        }

        public override int GetHashCode(){
            return _list != null ? _list.GetHashCode() : 0;
        }
    }

    public static class ReadOnlyListExt{
        public static ReadOnlyList<T> ToReadOnlyList<T>(this List<T> source){
            return new ReadOnlyList<T>(source);
        }

        /// 转换为池化的list
        public static PoolLinq.ListSource<T> ToPoolList<T>(this ReadOnlyList<T> source){
            var list = ListPool<T>.Get();
            foreach (var x in source)
                list.Add(x);
            return new PoolLinq.ListSource<T>(list);
        }

        /// 转换为list
        public static List<T> ToList<T>(this ReadOnlyList<T> source){
            var list = new List<T>(source.Count);
            foreach (var x in source)
                list.Add(x);
            return list;
        }
    }
}