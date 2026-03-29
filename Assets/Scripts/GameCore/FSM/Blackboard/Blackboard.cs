using System.Collections.Generic;
using Tools.ReferencePool;

namespace GameCore.FSM{
    /// <summary>
    ///     有限状态机黑板类，用于存储状态机运行时的数据，支持引用池管理
    /// </summary>
    public class Blackboard : IReference{
        /// <summary>
        ///     存储所有黑板值的字典
        /// </summary>
        private readonly Dictionary<string, BlackboardValue> _values = new();

        /// <summary>
        ///     设置黑板值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="name">值名称</param>
        /// <param name="value">值内容</param>
        public void SetValue<T>(string name, T value){
            if (_values.ContainsKey(name)){
                if (_values[name] is BlackboardValue<T> existingValue){
                    existingValue.Value = value;
                }
                else{
                    var newValue = ReferencePool.Acquire<BlackboardValue<T>>();
                    newValue.Name = name;
                    newValue.Value = value;
                    _values[name] = newValue;
                }
            }
            else{
                var newValue = ReferencePool.Acquire<BlackboardValue<T>>();
                newValue.Name = name;
                newValue.Value = value;
                _values.Add(name, newValue);
            }
        }

        /// <summary>
        ///     获取黑板值
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="name">值名称</param>
        /// <returns>值内容</returns>
        public T GetValue<T>(string name){
            if (_values.TryGetValue(name, out var value)){
                if (value is BlackboardValue<T> typedValue){
                    return typedValue.Value;
                }
            }

            return default;
        }

        /// <summary>
        ///     检查是否存在指定名称的值
        /// </summary>
        /// <param name="name">值名称</param>
        /// <returns>是否存在</returns>
        public bool HasValue(string name){
            return _values.ContainsKey(name);
        }

        /// <summary>
        ///     移除指定名称的值
        /// </summary>
        /// <param name="name">值名称</param>
        public void RemoveValue(string name){
            if (_values.TryGetValue(name, out var value)){
                value.Clear();
                ReferencePool.Release(value);
                _values.Remove(name);
            }
        }

        /// <summary>
        ///     清空所有值
        /// </summary>
        public void Clear(){
            foreach (var value in _values.Values){
                value.Clear();
                ReferencePool.Release(value);
            }

            _values.Clear();
        }
    }
}