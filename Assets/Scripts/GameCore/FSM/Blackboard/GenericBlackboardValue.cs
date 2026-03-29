using System;
using Tools.ReferencePool;

namespace GameCore.FSM{
    /// <summary>
    ///     泛型黑板值实现类，支持引用池管理
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    public class BlackboardValue<T> : BlackboardValue{
        /// <summary>
        ///     实际存储的值
        /// </summary>
        public T Value{ get; set; }

        /// <summary>
        ///     值类型
        /// </summary>
        public override Type ValueType => typeof(T);

        /// <summary>
        ///     默认构造函数
        /// </summary>
        public BlackboardValue(){
            Name = string.Empty;
        }

        /// <summary>
        ///     带参数构造函数
        /// </summary>
        /// <param name="name">值名称</param>
        /// <param name="value">初始值</param>
        public BlackboardValue(string name, T value){
            Name = name;
            Value = value;
        }

        /// <summary>
        ///     清理值内容，实现 IReference 接口
        /// </summary>
        public override void Clear(){
            if (Value is IDisposable disposable){
                disposable.Dispose();
            }

            Value = default;
            Name = string.Empty;
        }

        /// <summary>
        ///     克隆值对象
        /// </summary>
        /// <returns>克隆的值对象</returns>
        public override BlackboardValue Clone(){
            var clone = ReferencePool.Acquire<BlackboardValue<T>>();
            clone.Name = Name;
            clone.Value = Value;
            return clone;
        }
    }
}