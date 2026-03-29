using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.Pool;

namespace Tools.PoolLinq{
    /*
    public static class PoolLinqDemo
    {
        private class Parent
        {
            public Basic obj { get; set; }
        }
        private class Basic
        {
            public int Id { get; set; }
        }
        private class Subclass : Basic
        {
            public string Name { get; set; }
        }
        private static List<Subclass> Main()
        {
            // 创建一个测试列表
            var list = new List<Parent>();
            // Select对象的obj字段，然后筛选出Id不为0的，然后转换为Subclass类型
            var source = list.PoolSelect(a=>a.obj).PoolWhere(a=>a.Id !=0).PoolOfType<Basic,Subclass>();
            // 迭代source
            foreach (var subclass in source.List)
            {
                TLog.Log(subclass.Name);
            }
            // 使用source中的列表创建一个list2
            var list2 = new List<Subclass>(source.List);
            // 释放source
            source.ReleaseList();
            // 把list2也转换为新的List（替换 Enumerable.ToList(list2) 的）
            source = list2.ToPoolList();
            // 使用source创建一个list3，同时指定在下一帧释放source（用Unitask等待一帧，延迟释放，为了先在当前帧可以使用一会儿）
            var list3 = new List<Subclass>(source.DelayReleaseList());
            // 因为source将在下一帧释放，所以不用调 ReleaseList
            // 把list2也转换为新的List
            source = list3.ToPoolList();
            // source转换为常规的List，同时释放source
            // 常规List返回出去，在外面使用
            return source.ToGCList();
        }
    }
    */
    public static class PoolLinq{
        /// 自定义linq返回的list操作源
        public struct ListSource<T>{
            public readonly List<T> List;

            internal ListSource(List<T> list){
                List = list;
            }
        }

        public static ListSource<R> PoolSelect<T, R>(this List<T> source, Func<T, R> selector){
            var list = ListPool<R>.Get();
            foreach (var value in source){
                var result = selector(value);
                list.Add(result);
            }

            return new ListSource<R>(list);
        }

        public static ListSource<R> PoolSelect<T, R>(this ReadOnlyList<T> source, Func<T, R> selector){
            var list = ListPool<R>.Get();
            foreach (var value in source){
                var result = selector(value);
                list.Add(result);
            }

            return new ListSource<R>(list);
        }

        public static ListSource<R> PoolSelect<T, R>(this T[] source, Func<T, R> selector){
            var list = ListPool<R>.Get();
            foreach (var value in source){
                var result = selector(value);
                list.Add(result);
            }

            return new ListSource<R>(list);
        }

        public static ListSource<R> PoolSelect<T, V, R>(this Dictionary<T, V> source,
            Func<KeyValuePair<T, V>, R> selector){
            var list = ListPool<R>.Get();
            foreach (var value in source){
                var result = selector(value);
                list.Add(result);
            }

            return new ListSource<R>(list);
        }

        public static ListSource<R> PoolSelect<T, V, R>(this ReadOnlyDictionary<T, V> source,
            Func<KeyValuePair<T, V>, R> selector){
            var list = ListPool<R>.Get();
            foreach (var value in source){
                var result = selector(value);
                list.Add(result);
            }

            return new ListSource<R>(list);
        }

        public static ListSource<R> PoolSelect<T, R>(this ListSource<T> source, Func<T, R> selector){
            var result = PoolSelect(source.List, selector);
            ReleaseList(source);
            return result;
        }

        public static ListSource<R> PoolSelectMany<R>(this ListSource<List<R>> source){
            var list = ListPool<R>.Get();
            foreach (var value in source.List){
                list.AddRange(value);
            }

            source.ReleaseList();
            return new ListSource<R>(list);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this Dictionary<T, List<R>> source,
            Func<KeyValuePair<T, List<R>>, List<R>> selector){
            var list = ListPool<R>.Get();
            foreach (var value in source){
                var result = selector(value);
                list.AddRange(result);
            }

            return new ListSource<R>(list);
        }

// @formatter:off
        // 分别是6个支持的类型展开6个支持的类型，一共36个公开的签名
        public static ListSource<R> PoolSelectMany<T, R>(this ListSource<T> source, Func<T, ListSource<R>> selector){
            return source.DelayReleaseList().PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this ListSource<T> source, Func<T, List<R>> selector){
            return source.DelayReleaseList().PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this ListSource<T> source, Func<T, ReadOnlyList<R>> selector){
            return source.DelayReleaseList().PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this ListSource<T> source, Func<T, R[]> selector){
            return source.DelayReleaseList().PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<KeyValuePair<R, RV>> PoolSelectMany<T, R, RV>(this ListSource<T> source, Func<T, Dictionary<R,RV>> selector){
            return source.DelayReleaseList().PoolSelectManyDelegate<T, R, RV>(selector);
        }

        public static ListSource<KeyValuePair<R, RV>> PoolSelectMany<T, R, RV>(this ListSource<T> source, Func<T, ReadOnlyDictionary<R,RV>> selector){
            return source.DelayReleaseList().PoolSelectManyDelegate<T, R, RV>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this List<T> source, Func<T, ListSource<R>> selector){
            return source.PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this List<T> source, Func<T, List<R>> selector){
            return source.PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this List<T> source, Func<T, ReadOnlyList<R>> selector){
            return source.PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this List<T> source, Func<T, R[]> selector){
            return source.PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<KeyValuePair<R, RV>> PoolSelectMany<T, R, RV>(this List<T> source, Func<T, Dictionary<R,RV>> selector){
            return source.PoolSelectManyDelegate<T, R, RV>(selector);
        }

        public static ListSource<KeyValuePair<R, RV>> PoolSelectMany<T, R, RV>(this List<T> source, Func<T, ReadOnlyDictionary<R,RV>> selector){
            return source.PoolSelectManyDelegate<T, R, RV>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this ReadOnlyList<T> source, Func<T, ListSource<R>> selector){
            return ReadOnlyListExt.ToPoolList(source).DelayReleaseList().PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this ReadOnlyList<T> source, Func<T, List<R>> selector){
            return ReadOnlyListExt.ToPoolList(source).DelayReleaseList().PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this ReadOnlyList<T> source, Func<T, ReadOnlyList<R>> selector){
            return ReadOnlyListExt.ToPoolList(source).DelayReleaseList().PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this ReadOnlyList<T> source, Func<T, R[]> selector){
            return ReadOnlyListExt.ToPoolList(source).DelayReleaseList().PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<KeyValuePair<R, RV>> PoolSelectMany<T, R, RV>(this ReadOnlyList<T> source, Func<T, Dictionary<R,RV>> selector){
            return ReadOnlyListExt.ToPoolList(source).DelayReleaseList().PoolSelectManyDelegate<T, R, RV>(selector);
        }

        public static ListSource<KeyValuePair<R, RV>> PoolSelectMany<T, R, RV>(this ReadOnlyList<T> source, Func<T, ReadOnlyDictionary<R,RV>> selector){
            return ReadOnlyListExt.ToPoolList(source).DelayReleaseList().PoolSelectManyDelegate<T, R, RV>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this T[] source, Func<T, ListSource<R>> selector){
            return source.PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this T[] source, Func<T, List<R>> selector){
            return source.PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this T[] source, Func<T, ReadOnlyList<R>> selector){
            return source.PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, R>(this T[] source, Func<T, R[]> selector){
            return source.PoolSelectManyDelegate<T, R>(selector);
        }

        public static ListSource<KeyValuePair<R, RV>> PoolSelectMany<T, R, RV>(this T[] source, Func<T, Dictionary<R,RV>> selector){
            return source.PoolSelectManyDelegate<T, R, RV>(selector);
        }

        public static ListSource<KeyValuePair<R, RV>> PoolSelectMany<T, R, RV>(this T[] source, Func<T, ReadOnlyDictionary<R,RV>> selector){
            return source.PoolSelectManyDelegate<T, R, RV>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, V, R>(this Dictionary<T, V> source, Func<KeyValuePair<T, V>, ListSource<R>> selector){
            return source.PoolSelectManyDelegate<T, V, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, V, R>(this Dictionary<T, V> source, Func<KeyValuePair<T, V>, List<R>> selector){
            return source.PoolSelectManyDelegate<T, V, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, V, R>(this Dictionary<T, V> source, Func<KeyValuePair<T, V>, ReadOnlyList<R>> selector){
            return source.PoolSelectManyDelegate<T, V, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, V, R>(this Dictionary<T, V> source, Func<KeyValuePair<T, V>, R[]> selector){
            return source.PoolSelectManyDelegate<T, V, R>(selector);
        }

        public static ListSource<KeyValuePair<R, RV>> PoolSelectMany<T, V, R, RV>(this Dictionary<T, V> source, Func<KeyValuePair<T, V>, Dictionary<R,RV>> selector){
            return source.PoolSelectManyDelegate<T, V, R, RV>(selector);
        }

        public static ListSource<KeyValuePair<R, RV>> PoolSelectMany<T, V, R, RV>(this Dictionary<T, V> source, Func<KeyValuePair<T, V>, ReadOnlyDictionary<R,RV>> selector){
            return source.PoolSelectManyDelegate<T, V, R, RV>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, V, R>(this ReadOnlyDictionary<T, V> source, Func<KeyValuePair<T, V>, ListSource<R>> selector){
            return source.ToPoolList().DelayReleaseList().PoolSelectManyDelegate<T, V, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, V, R>(this ReadOnlyDictionary<T, V> source, Func<KeyValuePair<T, V>, List<R>> selector){
            return source.ToPoolList().DelayReleaseList().PoolSelectManyDelegate<T, V, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, V, R>(this ReadOnlyDictionary<T, V> source, Func<KeyValuePair<T, V>, ReadOnlyList<R>> selector){
            return source.ToPoolList().DelayReleaseList().PoolSelectManyDelegate<T, V, R>(selector);
        }

        public static ListSource<R> PoolSelectMany<T, V, R>(this ReadOnlyDictionary<T, V> source, Func<KeyValuePair<T, V>, R[]> selector){
            return source.ToPoolList().DelayReleaseList().PoolSelectManyDelegate<T, V, R>(selector);
        }

        public static ListSource<KeyValuePair<R, RV>> PoolSelectMany<T, V, R, RV>(this ReadOnlyDictionary<T, V> source, Func<KeyValuePair<T, V>, Dictionary<R,RV>> selector){
            return source.ToPoolList().DelayReleaseList().PoolSelectManyDelegate<T, V, R, RV>(selector);
        }

        public static ListSource<KeyValuePair<R, RV>> PoolSelectMany<T, V, R, RV>(this ReadOnlyDictionary<T, V> source, Func<KeyValuePair<T, V>, ReadOnlyDictionary<R,RV>> selector){
            return source.ToPoolList().DelayReleaseList().PoolSelectManyDelegate<T, V, R, RV>(selector);
        }

        // 列表到可枚举列表
        private static ListSource<R> PoolSelectManyDelegate<T, R>(this IEnumerable<T> source, Delegate selector)
        {
            var list = ListPool<R>.Get();
            switch (source)
            {
                case List<T> listSource: foreach (var r in listSource) SelectManyFormT(r); break;
                case T[] listSource: foreach (var r in listSource) SelectManyFormT(r); break;
                default: throw new ArgumentException();
            }

            return new ListSource<R>(list);

            void SelectManyFormT(T value)
            {
                switch (selector)
                {
                    case Func<T, List<R>> listSource: foreach (var r in listSource(value)) list.Add(r); break;
                    case Func<T, ReadOnlyList<R>> listSource: foreach (var r in listSource(value)) list.Add(r); break;
                    case Func<T, ListSource<R>> listSource: foreach (var r in listSource(value).List) list.Add(r); break;
                    case Func<T, R[]> listSource: foreach (var r in listSource(value)) list.Add(r); break;
                }
            }
        }
        // 字典到可枚举列表
        private static ListSource<R> PoolSelectManyDelegate<T, V, R>(this IEnumerable<KeyValuePair<T, V>> source, Delegate selector)
        {
            var list = ListPool<R>.Get();
            switch (source)
            {
                case List<KeyValuePair<T, V>> listSource: foreach (var r in listSource) SelectManyFormT(r); break;
                case KeyValuePair<T, V>[] listSource: foreach (var r in listSource) SelectManyFormT(r); break;
                case Dictionary<T, V> listSource: foreach (var r in listSource) SelectManyFormT(r); break;
                default: throw new ArgumentException();
            }

            return new ListSource<R>(list);

            void SelectManyFormT(KeyValuePair<T, V> value)
            {
                switch (selector)
                {
                    case Func<KeyValuePair<T, V>, List<R>> listSource: foreach (var r in listSource(value)) list.Add(r); break;
                    case Func<KeyValuePair<T, V>, ReadOnlyList<R>> listSource: foreach (var r in listSource(value)) list.Add(r); break;
                    case Func<KeyValuePair<T, V>, ListSource<R>> listSource: foreach (var r in listSource(value).List) list.Add(r); break;
                    case Func<KeyValuePair<T, V>, R[]> listSource: foreach (var r in listSource(value)) list.Add(r); break;
                }
            }
        }
        // 列表到可枚举字典
        private static ListSource<KeyValuePair<R, RV>> PoolSelectManyDelegate<T, R, RV>(this IEnumerable<T> source, Delegate selector)
        {
            var list = ListPool<KeyValuePair<R, RV>>.Get();
            switch (source)
            {
                case List<T> listSource: foreach (var r in listSource) SelectManyFormT(r); break;
                case T[] listSource: foreach (var r in listSource) SelectManyFormT(r); break;
                default: throw new ArgumentException();
            }

            void SelectManyFormT(T value)
            {
                switch (selector)
                {
                    case Func<T, Dictionary<R, RV>> listSource: foreach (var r in listSource(value)) list.Add(r); break;
                    case Func<T, ReadOnlyDictionary<R, RV>> listSource: foreach (var r in listSource(value)) list.Add(r); break;
                }
            }

            return new ListSource<KeyValuePair<R, RV>>(list);
        }
        // 字典到可枚举字典
        private static ListSource<KeyValuePair<R, RV>> PoolSelectManyDelegate<T, V, R, RV>(this IEnumerable<KeyValuePair<T, V>> source, Delegate selector)
        {
            var list = ListPool<KeyValuePair<R, RV>>.Get();
            switch (source)
            {
                case List<KeyValuePair<T, V>> listSource: foreach (var r in listSource) SelectManyFormT(r); break;
                case KeyValuePair<T, V>[] listSource: foreach (var r in listSource) SelectManyFormT(r); break;
                case Dictionary<T, V> listSource: foreach (var r in listSource) SelectManyFormT(r); break;
                default: throw new ArgumentException();
            }

            void SelectManyFormT(KeyValuePair<T, V> value)
            {
                switch (selector)
                {
                    case Func<KeyValuePair<T, V>, Dictionary<R, RV>> listSource: foreach (var r in listSource(value)) list.Add(r); break;
                    case Func<KeyValuePair<T, V>, ReadOnlyDictionary<R, RV>> listSource: foreach (var r in listSource(value)) list.Add(r); break;
                }
            }

            return new ListSource<KeyValuePair<R, RV>>(list);
        }
        // @formatter:on

        public static T PoolMax<T>(this ListSource<T> source) where T : struct{
            if (source.List.Count <= 0)
                throw new SystemException("List count is 0.");

            var v = source.List[0];
            foreach (var value in source.List){
                if (value.GetHashCode() > v.GetHashCode()) v = value;
            }

            return v;
        }

        public static T PoolMax<T>(this List<T> source) where T : struct{
            if (source.Count <= 0)
                throw new SystemException("List count is 0.");

            var v = source[0];
            foreach (var value in source){
                if (value.GetHashCode() > v.GetHashCode()) v = value;
            }

            return v;
        }

        public static T PoolMax<T>(this ReadOnlyList<T> source) where T : struct{
            if (source.Count <= 0)
                throw new SystemException("List count is 0.");

            var v = source[0];
            foreach (var value in source){
                if (value.GetHashCode() > v.GetHashCode()) v = value;
            }

            return v;
        }

        public static T PoolMax<T>(this T[] source) where T : struct{
            if (source.Length <= 0)
                throw new SystemException("List count is 0.");

            var v = source[0];
            foreach (var value in source){
                if (value.GetHashCode() > v.GetHashCode()) v = value;
            }

            return v;
        }

        public static ListSource<T> PoolWhere<T>(this ReadOnlyList<T> source, Func<T, bool> whereFunc){
            var list = ListPool<T>.Get();
            foreach (var value in source){
                if (whereFunc(value))
                    list.Add(value);
            }

            return new ListSource<T>(list);
        }

        public static ListSource<T> PoolWhere<T>(this List<T> source, Func<T, bool> whereFunc){
            var list = ListPool<T>.Get();
            foreach (var value in source){
                if (whereFunc(value))
                    list.Add(value);
            }

            return new ListSource<T>(list);
        }

        public static ListSource<T> PoolWhere<T>(this T[] source, Func<T, bool> whereFunc){
            var list = ListPool<T>.Get();
            foreach (var value in source){
                if (whereFunc(value))
                    list.Add(value);
            }

            return new ListSource<T>(list);
        }

        public static ListSource<KeyValuePair<T, V>> PoolWhere<T, V>(this Dictionary<T, V> source,
            Func<KeyValuePair<T, V>, bool> whereFunc){
            var list = ListPool<KeyValuePair<T, V>>.Get();
            foreach (var value in source){
                if (whereFunc(value))
                    list.Add(value);
            }

            return new ListSource<KeyValuePair<T, V>>(list);
        }

        public static ListSource<KeyValuePair<T, V>> PoolWhere<T, V>(this ReadOnlyDictionary<T, V> source,
            Func<KeyValuePair<T, V>, bool> whereFunc){
            var list = ListPool<KeyValuePair<T, V>>.Get();
            foreach (var value in source){
                if (whereFunc(value))
                    list.Add(value);
            }

            return new ListSource<KeyValuePair<T, V>>(list);
        }

        public static ListSource<T> PoolWhere<T>(this ListSource<T> source, Func<T, bool> whereFunc){
            var result = PoolWhere(source.List, whereFunc);
            ReleaseList(source);
            return result;
        }

        public static ListSource<T> PoolSubList<T>(this ReadOnlyList<T> source, int index, int count){
            var list = ListPool<T>.Get();
            for (var i = 0; i < source.Count; i++){
                if (count == 0)
                    break;
                if (i < index)
                    continue;
                count--;
                var value = source[i];
                list.Add(value);
            }

            return new ListSource<T>(list);
        }

        public static ListSource<T> PoolSubList<T>(this List<T> source, int index, int count){
            var list = ListPool<T>.Get();
            for (var i = 0; i < source.Count; i++){
                if (count == 0)
                    break;
                if (i < index)
                    continue;
                count--;
                var value = source[i];
                list.Add(value);
            }

            return new ListSource<T>(list);
        }

        public static ListSource<T> PoolSubList<T>(this T[] source, int index, int count){
            var list = ListPool<T>.Get();
            for (var i = 0; i < source.Length; i++){
                if (count == 0)
                    break;
                if (i < index)
                    continue;
                count--;
                var value = source[i];
                list.Add(value);
            }

            return new ListSource<T>(list);
        }

        public static ListSource<T> PoolSubList<T>(this ListSource<T> source, int index, int count){
            var result = PoolSubList(source.List, index, count);
            ReleaseList(source);
            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T">源类型</typeparam>
        /// <typeparam name="R">要转换到的类型</typeparam>
        /// <returns></returns>
        public static ListSource<R> PoolOfType<T, R>(this ReadOnlyList<T> source){
            var list = ListPool<R>.Get();
            foreach (var value in source){
                if (value is R r)
                    list.Add(r);
            }

            return new ListSource<R>(list);
        }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T">源类型</typeparam>
        /// <typeparam name="R">要转换到的类型</typeparam>
        /// <returns></returns>
        public static ListSource<R> PoolOfType<T, R>(this List<T> source){
            var list = ListPool<R>.Get();
            foreach (var value in source){
                if (value is R r)
                    list.Add(r);
            }

            return new ListSource<R>(list);
        }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T">源类型</typeparam>
        /// <typeparam name="R">要转换到的类型</typeparam>
        /// <returns></returns>
        public static ListSource<R> PoolOfType<T, R>(this ListSource<T> source){
            var result = PoolOfType<T, R>(source.List);
            ReleaseList(source);
            return result;
        }

        [CanBeNull]
        public static T NoGCFirstOrDefault<T>(this ReadOnlyList<T> source, Func<T, bool> predicate = null){
            foreach (var source1 in source){
                if (predicate == null)
                    return source1;
                if (predicate(source1))
                    return source1;
            }

            return default;
        }

        [CanBeNull]
        public static T NoGCFirstOrDefault<T>(this List<T> source, Func<T, bool> predicate = null){
            foreach (var source1 in source){
                if (predicate == null)
                    return source1;
                if (predicate(source1))
                    return source1;
            }

            return default;
        }

        [CanBeNull]
        public static T NoGCFirstOrDefault<T>(this T[] source, Func<T, bool> predicate = null){
            foreach (var source1 in source){
                if (predicate == null)
                    return source1;
                if (predicate(source1))
                    return source1;
            }

            return default;
        }

        public static KeyValuePair<T, V> NoGCFirstOrDefault<T, V>(this Dictionary<T, V> source,
            Func<KeyValuePair<T, V>, bool> predicate = null){
            foreach (var source1 in source){
                if (predicate == null)
                    return source1;
                if (predicate(source1))
                    return source1;
            }

            return default;
        }

        public static KeyValuePair<T, V> NoGCFirstOrDefault<T, V>(this ReadOnlyDictionary<T, V> source,
            Func<KeyValuePair<T, V>, bool> predicate = null){
            foreach (var source1 in source){
                if (predicate == null)
                    return source1;
                if (predicate(source1))
                    return source1;
            }

            return default;
        }

        public static T NoGCFirstOrDefault<T>(this ListSource<T> source, Func<T, bool> predicate = null){
            foreach (var source1 in source.List){
                if (predicate == null)
                    return source1;
                if (predicate(source1))
                    return source1;
            }

            return default;
        }

        public static bool NoGCAny<T>(this ReadOnlyList<T> source, Func<T, bool> predicate){
            foreach (var source1 in source){
                if (predicate(source1))
                    return true;
            }

            return false;
        }

        public static bool NoGCAny<T>(this List<T> source, Func<T, bool> predicate){
            foreach (var source1 in source){
                if (predicate(source1))
                    return true;
            }

            return false;
        }

        public static bool NoGCAny<T>(this T[] source, Func<T, bool> predicate){
            foreach (var source1 in source){
                if (predicate(source1))
                    return true;
            }

            return false;
        }

        public static bool NoGCAny<T, V>(this Dictionary<T, V> source, Func<KeyValuePair<T, V>, bool> predicate){
            foreach (var source1 in source){
                if (predicate(source1))
                    return true;
            }

            return false;
        }

        public static bool NoGCAny<T, V>(this ReadOnlyDictionary<T, V> source,
            Func<KeyValuePair<T, V>, bool> predicate){
            foreach (var source1 in source){
                if (predicate(source1))
                    return true;
            }

            return false;
        }

        public static bool NoGCAny<T>(this ListSource<T> source, Func<T, bool> predicate){
            foreach (var source1 in source.List){
                if (predicate(source1))
                    return true;
            }

            return false;
        }

        public static int NoGCCount<T>(this ReadOnlyList<T> source, Func<T, bool> predicate = null){
            if (predicate == null)
                return source.Count;
            var count = 0;
            foreach (var source1 in source){
                if (predicate(source1))
                    count++;
            }

            return count;
        }

        public static int NoGCCount<T>(this List<T> source, Func<T, bool> predicate = null){
            if (predicate == null)
                return source.Count;
            var count = 0;
            foreach (var source1 in source){
                if (predicate(source1))
                    count++;
            }

            return count;
        }

        public static int NoGCCount<T>(this T[] source, Func<T, bool> predicate = null){
            if (predicate == null)
                return source.Length;
            var count = 0;
            foreach (var source1 in source){
                if (predicate(source1))
                    count++;
            }

            return count;
        }

        public static int NoGCCount<T, V>(this Dictionary<T, V> source,
            Func<KeyValuePair<T, V>, bool> predicate = null){
            if (predicate == null)
                return source.Count;
            var count = 0;
            foreach (var source1 in source){
                if (predicate(source1))
                    count++;
            }

            return count;
        }

        public static int NoGCCount<T, V>(this ReadOnlyDictionary<T, V> source,
            Func<KeyValuePair<T, V>, bool> predicate = null){
            if (predicate == null)
                return source.Count;
            var count = 0;
            foreach (var source1 in source){
                if (predicate(source1))
                    count++;
            }

            return count;
        }

        public static int NoGCCount<T>(this ListSource<T> source, Func<T, bool> predicate = null){
            if (predicate == null)
                return source.List.Count;
            var count = 0;
            foreach (var source1 in source.List){
                if (predicate(source1))
                    count++;
            }

            return count;
        }

        public static object GCElementAt(this Array source, int index){
            foreach (var k in source){
                if (index == 0){
                    return k;
                }

                index--;
            }

            return default;
        }

        public static TKey NoGCElementAt<TKey, TValue>(this Dictionary<TKey, TValue>.KeyCollection source, int index){
            foreach (var k in source){
                if (index == 0){
                    return k;
                }

                index--;
            }

            return default;
        }

        public static TValue NoGCElementAt<TKey, TValue>(this Dictionary<TKey, TValue>.ValueCollection source,
            int index){
            foreach (var k in source){
                if (index == 0){
                    return k;
                }

                index--;
            }

            return default;
        }

        public static Dictionary<T, V> ListToDictionary<S, T, V>(this ReadOnlyList<S> source, Func<S, T> keySelector,
            Func<S, V> elementSelector){
            var dictionary = new Dictionary<T, V>();
            foreach (var source1 in source)
                dictionary.Add(keySelector(source1), elementSelector(source1));
            return dictionary;
        }

        public static Dictionary<T, V> ListToDictionary<S, T, V>(this List<S> source, Func<S, T> keySelector,
            Func<S, V> elementSelector){
            var dictionary = new Dictionary<T, V>();
            foreach (var source1 in source)
                dictionary.Add(keySelector(source1), elementSelector(source1));
            return dictionary;
        }

        public static Dictionary<T, V> ArrayToDictionary<S, T, V>(this S[] source, Func<S, T> keySelector,
            Func<S, V> elementSelector){
            var dictionary = new Dictionary<T, V>();
            foreach (var source1 in source)
                dictionary.Add(keySelector(source1), elementSelector(source1));
            return dictionary;
        }

        public static Dictionary<T, V> DictionaryToDictionary<ST, SV, T, V>(this Dictionary<ST, SV> source,
            Func<KeyValuePair<ST, SV>, T> keySelector, Func<KeyValuePair<ST, SV>, V> elementSelector){
            var dictionary = new Dictionary<T, V>();
            foreach (var source1 in source)
                dictionary.Add(keySelector(source1), elementSelector(source1));
            return dictionary;
        }

        public static Dictionary<T, V> DictionaryToDictionary<ST, SV, T, V>(this ReadOnlyDictionary<ST, SV> source,
            Func<KeyValuePair<ST, SV>, T> keySelector, Func<KeyValuePair<ST, SV>, V> elementSelector){
            var dictionary = new Dictionary<T, V>();
            foreach (var source1 in source)
                dictionary.Add(keySelector(source1), elementSelector(source1));
            return dictionary;
        }

        /// 释放这个列表
        public static void ReleaseList<T>(this ListSource<T> source){
            if (source.List == null)
                return;
            ListPool<T>.Release(source.List);
        }

        /// 释放这个列表
        public static ListSource<T> CreatePoolList<T>(){
            var list = ListPool<T>.Get();
            return new ListSource<T>(list);
        }

        /// 延迟释放这个列表，将在下一帧释放，为了再临时使用一下下
        /// <br />
        /// 比如当做临时参数传递时
        /// <br />
        /// var list3 = new List(source.DelayReleaseList());
        public static List<T> DelayReleaseList<T>(this ListSource<T> source){
            YieldReleaseList(source);
            return source.List;
        }

        /// 转换为池化的list
        public static ListSource<T> ToPoolList<T>(this IEnumerable<T> source){
            var list = ListPool<T>.Get();
            list.AddRange(source);
            return new ListSource<T>(list);
        }

        /// 池化list创建一个新的list并返回
        public static List<T> ToGCList<T>(this ListSource<T> source, List<T> list = null){
            if (list == null){
                list = new List<T>(source.List.Count);
            }
            else{
                list.Clear();
            }

            list.AddRange(source.List);
            ReleaseList(source);
            return list;
        }

        /// 池化list创建一个新的array并返回
        public static T[] ToGCArray<T>(this ListSource<T> source){
            if (source.List.Count == 0){
                ReleaseList(source);
                return Array.Empty<T>();
            }

            var array = new T[source.List.Count];
            for (var i = 0; i < source.List.Count; i++)
                array[i] = source.List[i];
            ReleaseList(source);
            return array;
        }

        private static async void YieldReleaseList<T>(this ListSource<T> source){
            await UniTask.Yield();
            source.ReleaseList();
        }
    }
}