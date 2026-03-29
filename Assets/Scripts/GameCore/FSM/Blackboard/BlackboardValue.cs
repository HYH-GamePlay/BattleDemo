using System;
using Tools.ReferencePool;

namespace GameCore.FSM{
    /// <summary>
    ///     有限状态机黑板值基类，用于在状态机中存储和传递数据
    /// </summary>
    public abstract class BlackboardValue : IReference{
        /// <summary>
        ///     值的名称
        /// </summary>
        public string Name{ get; set; }

        /// <summary>
        ///     值的类型
        /// </summary>
        public abstract Type ValueType{ get; }

        /// <summary>
        ///     清理值内容，实现 IReference 接口
        /// </summary>
        public abstract void Clear();

        /// <summary>
        ///     克隆值对象
        /// </summary>
        /// <returns>克隆的值对象</returns>
        public abstract BlackboardValue Clone();
    }
}