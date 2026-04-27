using System;

namespace GameCore.Core.Comp.Event{
    public interface IEventComp : IComp{
        /// <summary>
        /// 添加事件监听器（带参数）
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="eventId">事件ID</param>
        /// <param name="handler">事件处理器</param>
        void AddListener<T>(EventId eventId, Action<T> handler);

        /// <summary>
        /// 移除事件监听器（带参数）
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="eventId">事件ID</param>
        /// <param name="handler">事件处理器</param>
        void RemoveListener<T>(EventId eventId, Action<T> handler);

        /// <summary>
        /// 广播事件（带参数）
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="eventId">事件ID</param>
        /// <param name="data">事件数据</param>
        void Broadcast<T>(EventId eventId, T data);

        /// <summary>
        /// 添加事件监听器（无参数）
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="handler">事件处理器</param>
        void AddListener(EventId eventId, Action handler);

        /// <summary>
        /// 移除事件监听器（无参数）
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="handler">事件处理器</param>
        void RemoveListener(EventId eventId, Action handler);

        /// <summary>
        /// 广播事件（无参数）
        /// </summary>
        /// <param name="eventId">事件ID</param>
        void Broadcast(EventId eventId);

        /// <summary>
        /// 清理指定事件的所有监听器
        /// </summary>
        /// <param name="eventId">事件ID</param>
        void ClearEvent(EventId eventId);

        /// <summary>
        /// 清理所有事件监听器
        /// </summary>
        void ClearAllEvents();

        /// <summary>
        /// 检查事件是否有监听器
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <returns>是否有监听器</returns>
        bool HasListener(EventId eventId);

        /// <summary>
        /// 获取事件监听器数量
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <returns>监听器数量</returns>
        int GetListenerCount(EventId eventId);

        /// <summary>
        /// 获取所有事件数量
        /// </summary>
        /// <returns>事件数量</returns>
        int GetEventCount();
    }
}