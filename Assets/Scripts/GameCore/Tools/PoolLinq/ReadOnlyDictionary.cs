using System;
using System.Collections;
using System.Collections.Generic;

namespace Tools.PoolLinq{
    // public class Demo
    // {
    //     public void Test()
    //     {
    //         Dictionary<int, int> a = new Dictionary<int, int>();
    //         ReadOnlyDictionary<int, int> read = new ReadOnlyDictionary<int, int>(a);
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
    //         Debug.Log(read.FirstOrDefault(x => x.Key != 0));
    //         // No GC， 转池化列表，linq避免GC
    //         Debug.Log(read.ToPoolList().DelayReleaseList().NoGCFirstOrDefault(x => x.Key != 0));
    //     }
    // }

    public readonly struct ReadOnlyDictionary<TK, TV> : IReadOnlyDictionary<TK, TV>,
        IEquatable<ReadOnlyDictionary<TK, TV>>{
        private readonly Dictionary<TK, TV> _dic;

        public int Count => _dic?.Count ?? 0;

        public TV this[TK key] => _dic == null ? default : _dic[key];
        public IEnumerable<TK> Keys => _dic.Keys;
        public IEnumerable<TV> Values => _dic.Values;

        public ReadOnlyDictionary(Dictionary<TK, TV> dic){
            _dic = dic;
        }

        public bool ContainsKey(TK key){
            return _dic.ContainsKey(key);
        }

        public bool TryGetValue(TK key, out TV value){
            return _dic.TryGetValue(key, out value);
        }

        public Dictionary<TK, TV>.Enumerator GetEnumerator(){
            return _dic?.GetEnumerator() ?? default;
        }

        IEnumerator<KeyValuePair<TK, TV>> IEnumerable<KeyValuePair<TK, TV>>.GetEnumerator(){
            return _dic?.GetEnumerator() ?? new Dictionary<TK, TV>.Enumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return _dic?.GetEnumerator() ?? new Dictionary<TK, TV>.Enumerator();
        }

        public bool IsValid(){
            return _dic != null;
        }

        public static bool operator ==(ReadOnlyDictionary<TK, TV> lhs, ReadOnlyDictionary<TK, TV> rhs){
            return lhs._dic == rhs._dic;
        }

        public static bool operator !=(ReadOnlyDictionary<TK, TV> lhs, ReadOnlyDictionary<TK, TV> rhs){
            return lhs._dic != rhs._dic;
        }

        public bool Equals(ReadOnlyDictionary<TK, TV> other){
            return Equals(_dic, other._dic);
        }

        public override bool Equals(object obj){
            return obj is ReadOnlyList<TK> other && Equals(other);
        }

        public override int GetHashCode(){
            return _dic != null ? _dic.GetHashCode() : 0;
        }
    }

    public static class ReadOnlyDictionaryExt{
        public static ReadOnlyDictionary<TK, TV> ToReadOnlyDictionary<TK, TV>(this Dictionary<TK, TV> source){
            return new ReadOnlyDictionary<TK, TV>(source);
        }

        /// 转换为字典
        public static Dictionary<TK, TV> ToDictionary<TK, TV>(this ReadOnlyDictionary<TK, TV> source){
            var dic = new Dictionary<TK, TV>(source.Count);
            foreach (var x in source)
                dic.Add(x.Key, x.Value);
            return dic;
        }
    }
}