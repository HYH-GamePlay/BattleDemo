using System;
using System.Collections.Generic;
using Tools.Log;
using Tools.Singleton;

namespace Tools.ReferencePool{
    public class ReferencePool : Singleton<ReferencePool>{
        private static readonly Dictionary<Type, ReferenceCollection> ReferenceCollections = new();

        private static int Count{
            get{
                lock (ReferenceCollections){
                    return ReferenceCollections?.Count ?? 0;
                }
            }
        }

        public static T Acquire<T>() where T : class, IReference, new(){
            return GetReferenceCollection(typeof(T))?.Acquire<T>();
        }

        public static void Release(IReference reference){
            GetReferenceCollection(reference?.GetType())?.Release(reference);
        }

        public static void Add<T>(int count) where T : class, IReference, new(){
            GetReferenceCollection(typeof(T))?.Add<T>(count);
        }

        public static void Remove(Type type, int count){
            GetReferenceCollection(type)?.Remove(count);
        }

        public static void RemoveAll(Type type){
            GetReferenceCollection(type)?.RemoveAll();
        }

        private static ReferenceCollection GetReferenceCollection(Type type){
            if (type == null) HLog.LogE("类型不能为空");

            ReferenceCollection referenceCollection = null;
            lock (ReferenceCollections){
                if (type != null && !ReferenceCollections.TryGetValue(type, out referenceCollection))
                    referenceCollection = ReferenceCollections[type] = new ReferenceCollection(type);
            }

            return referenceCollection;
        }
    }
}