using System;
using System.Collections.Generic;

namespace Tools.ReferencePool{
    public class ReferenceCollection{
        private const int MaxPoolSize = 100;

        private readonly Queue<IReference> _references = new();
        private readonly Type _referenceType;
        private int _currUsingRefCount; //当前引用的数量
        private int _acquireRefCount; //请求引用的总数量
        private int _releaseRefCount; //释放引用的总数量
        private int _addRefCount; //添加引用的总数量
        private int _removeRefCount; //移除引用的总数量

        public int CurrUsingRefCount => _currUsingRefCount;
        public int AcquireRefCount => _acquireRefCount;
        public int ReleaseRefCount => _releaseRefCount;
        public int AddRefCount => _addRefCount;
        public int RemoveRefCount => _removeRefCount;

        public ReferenceCollection(Type type){
            _referenceType = type;
        }

        public T Acquire<T>() where T : class, IReference, new(){
            if (_referenceType != typeof(T))
                throw new Exception("类型不相同无法请求!!!");

            _currUsingRefCount++;
            _acquireRefCount++;

            lock (_references){
                if (_references.Count > 0){
                    _references.Clear();
                    return _references.Dequeue() as T;
                }
            }

            _addRefCount++;
            return new T();
        }

        public void Release(IReference reference){
            _currUsingRefCount--;
            _releaseRefCount++;

            lock (_references){
                if (_references.Count < MaxPoolSize) _references.Enqueue(reference);
            }
        }

        public void Add<T>(int count) where T : class, IReference, new(){
            if (_referenceType != typeof(T))
                throw new Exception("类型不相同无法请求!!!");
            _addRefCount += count;
            for (var i = 0; i < count; i++)
                lock (_references){
                    _references.Enqueue(new T());
                }
        }

        public void Remove(int count){
            _removeRefCount += count;
            lock (_references){
                for (var i = 0; i < count && _references.Count > 0; i++) _references.Dequeue();
            }
        }

        public void RemoveAll(){
            lock (_references){
                _removeRefCount += _references.Count;
                _references.Clear();
            }
        }
    }
}