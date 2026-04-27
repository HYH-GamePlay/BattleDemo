using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Tools.Log;
using UnityEngine;

namespace GameCore.Core.Comp.Event{
    /// <summary>
    /// 事件组件
    /// 管理游戏内的事件系统
    /// </summary>
    public class EventComp : IEventComp{
        private Dictionary<EventId, List<Delegate>> _eventHandlerList = new();

        /// <summary>
        /// 初始化事件组件
        /// </summary>
        public UniTask Init()
        {
            if (_eventHandlerList != null)
            {
                HLog.LogW("EventComp already initialized!");
                return UniTask.CompletedTask;
            }

            _eventHandlerList = new Dictionary<EventId, List<Delegate>>();
            HLog.Log("EventComp initialized successfully!");
            
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 卸载事件组件
        /// </summary>
        public UniTask UnInit()
        {
            if (_eventHandlerList == null)
            {
                return UniTask.CompletedTask;
            }

            // 清理所有事件监听器
            _eventHandlerList.Clear();

            HLog.Log("EventComp uninitialized successfully!");
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 添加事件监听器（带参数）
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="eventId">事件ID</param>
        /// <param name="handler">事件处理器</param>
        public void AddListener<T>(EventId eventId, Action<T> handler)
        {
            if (handler == null)
            {
                HLog.LogE("Event handler is null!");
                return;
            }

            try
            {
                if (!_eventHandlerList.ContainsKey(eventId))
                {
                    _eventHandlerList[eventId] = new List<Delegate>();
                }

                _eventHandlerList[eventId].Add(handler);
                HLog.Log($"Added event listener: {eventId}");
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to add event listener {eventId}: {e.Message}");
            }
        }

        /// <summary>
        /// 移除事件监听器（带参数）
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="eventId">事件ID</param>
        /// <param name="handler">事件处理器</param>
        public void RemoveListener<T>(EventId eventId, Action<T> handler)
        {
            if (handler == null)
            {
                HLog.LogE("Event handler is null!");
                return;
            }

            try
            {
                if (_eventHandlerList.ContainsKey(eventId))
                {
                    _eventHandlerList[eventId].Remove(handler);
                    HLog.Log($"Removed event listener: {eventId}");
                }
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to remove event listener {eventId}: {e.Message}");
            }
        }

        /// <summary>
        /// 广播事件（带参数）
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="eventId">事件ID</param>
        /// <param name="data">事件数据</param>
        public void Broadcast<T>(EventId eventId, T data)
        {
            try
            {
                if (_eventHandlerList.ContainsKey(eventId))
                {
                    var handlers = _eventHandlerList[eventId];
                    if (handlers != null && handlers.Count > 0)
                    {
                        // 复制一份列表，避免在遍历时修改
                        var handlersCopy = new List<Delegate>(handlers);
                        foreach (var handler in handlersCopy)
                        {
                            if (handler is Action<T> action)
                            {
                                action?.Invoke(data);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to broadcast event {eventId}: {e.Message}");
            }
        }

        /// <summary>
        /// 添加事件监听器（无参数）
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="handler">事件处理器</param>
        public void AddListener(EventId eventId, Action handler)
        {
            if (handler == null)
            {
                HLog.LogE("Event handler is null!");
                return;
            }

            try
            {
                if (!_eventHandlerList.ContainsKey(eventId))
                {
                    _eventHandlerList[eventId] = new List<Delegate>();
                }

                _eventHandlerList[eventId].Add(handler);
                HLog.Log($"Added event listener: {eventId}");
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to add event listener {eventId}: {e.Message}");
            }
        }

        /// <summary>
        /// 移除事件监听器（无参数）
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <param name="handler">事件处理器</param>
        public void RemoveListener(EventId eventId, Action handler)
        {
            if (handler == null)
            {
                HLog.LogE("Event handler is null!");
                return;
            }

            try
            {
                if (_eventHandlerList.ContainsKey(eventId))
                {
                    _eventHandlerList[eventId].Remove(handler);
                    HLog.Log($"Removed event listener: {eventId}");
                }
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to remove event listener {eventId}: {e.Message}");
            }
        }

        /// <summary>
        /// 广播事件（无参数）
        /// </summary>
        /// <param name="eventId">事件ID</param>
        public void Broadcast(EventId eventId)
        {
            try
            {
                if (_eventHandlerList.ContainsKey(eventId))
                {
                    var handlers = _eventHandlerList[eventId];
                    if (handlers != null && handlers.Count > 0)
                    {
                        // 复制一份列表，避免在遍历时修改
                        var handlersCopy = new List<Delegate>(handlers);
                        foreach (var handler in handlersCopy)
                        {
                            if (handler is Action action)
                            {
                                action?.Invoke();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to broadcast event {eventId}: {e.Message}");
            }
        }

        /// <summary>
        /// 清理指定事件的所有监听器
        /// </summary>
        /// <param name="eventId">事件ID</param>
        public void ClearEvent(EventId eventId)
        {
            try
            {
                if (_eventHandlerList.ContainsKey(eventId))
                {
                    _eventHandlerList[eventId].Clear();
                    HLog.Log($"Cleared event: {eventId}");
                }
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to clear event {eventId}: {e.Message}");
            }
        }

        /// <summary>
        /// 清理所有事件监听器
        /// </summary>
        public void ClearAllEvents()
        {
            try
            {
                _eventHandlerList.Clear();
                HLog.Log("Cleared all events!");
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to clear all events: {e.Message}");
            }
        }

        /// <summary>
        /// 检查事件是否有监听器
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <returns>是否有监听器</returns>
        public bool HasListener(EventId eventId)
        {
            return _eventHandlerList.ContainsKey(eventId) && _eventHandlerList[eventId].Count > 0;
        }

        /// <summary>
        /// 获取事件监听器数量
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <returns>监听器数量</returns>
        public int GetListenerCount(EventId eventId)
        {
            if (_eventHandlerList.ContainsKey(eventId))
            {
                return _eventHandlerList[eventId].Count;
            }
            return 0;
        }

        /// <summary>
        /// 获取所有事件数量
        /// </summary>
        /// <returns>事件数量</returns>
        public int GetEventCount()
        {
            return _eventHandlerList.Count;
        }
    }
}