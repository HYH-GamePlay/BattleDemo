using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameCore.Core.Comp.Res{
    public interface IResComp : IComp{
        /// <summary>
        /// 加载资源（同步）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <returns>加载的资源</returns>
        T Load<T>(string path) where T : UnityEngine.Object;

        /// <summary>
        /// 加载资源（异步）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="callback">回调函数</param>
        void LoadAsync<T>(string path, Action<T> callback) where T : UnityEngine.Object;

        /// <summary>
        /// 异步加载资源（UniTask版本）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <returns>异步任务</returns>
        UniTask<T> LoadAsync<T>(string path) where T : UnityEngine.Object;

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="path">资源路径</param>
        void Release(string path);

        /// <summary>
        /// 释放资源（GameObject）
        /// </summary>
        /// <param name="gameObject">游戏对象</param>
        void Release(GameObject gameObject);

        /// <summary>
        /// 释放所有资源
        /// </summary>
        void ReleaseAll();

        /// <summary>
        /// 预加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="priority">优先级</param>
        void PreloadAsset(string path, int priority = 0);

        /// <summary>
        /// 预加载资源（异步）
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="callback">回调函数</param>
        /// <param name="priority">优先级</param>
        void PreloadAssetAsync(string path, Action<UnityEngine.Object> callback, int priority = 0);

        /// <summary>
        /// 卸载未使用的资源
        /// </summary>
        void UnloadUnusedAssets();

        /// <summary>
        /// 获取资源加载状态
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>加载状态</returns>
        string GetAssetLoadStatus(string path);

        /// <summary>
        /// 检查资源是否已加载
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>是否已加载</returns>
        bool IsAssetLoaded(string path);

        /// <summary>
        /// 获取已加载资源数量
        /// </summary>
        /// <returns>资源数量</returns>
        int GetLoadedAssetCount();
    }
}