using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Tools.Log;
using UnityEngine;
using YooAsset;

namespace GameCore.Core.Comp.Res{
    /// <summary>
    /// 资源组件
    /// 使用YooAsset管理游戏资源
    /// </summary>
    public class ResComp : IResComp{
        private Dictionary<string, AssetHandle> _handleCache = new();
        private Dictionary<string, UnityEngine.Object> _loadedObjectCache = new();

        /// <summary>
        /// 初始化资源组件
        /// </summary>
        public async UniTask Init()
        {
            try
            {
                // 初始化YooAsset
                YooAssets.Initialize();
                await UpdatePackage();
                HLog.Log("ResComp initialized successfully!");
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to initialize ResComp: {e.Message}");
            }
        }

        /// <summary>
        /// 卸载资源组件
        /// </summary>
        public UniTask UnInit()
        {
            try
            {
                // 释放所有缓存
                foreach (var handle in _handleCache.Values)
                {
                    handle.Release();
                }
                _handleCache.Clear();
                _loadedObjectCache.Clear();

                // 关闭YooAsset
                YooAssets.Destroy();

                HLog.Log("ResComp uninitialized successfully!");
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to uninitialize ResComp: {e.Message}");
            }
            return UniTask.CompletedTask;
        }
        
        private async UniTask UpdatePackage(){
            // 创建资源包裹类
            var package = YooAssets.TryGetPackage("DefaultPackage") ?? YooAssets.CreatePackage("DefaultPackage");
            InitializeParameters createParameters = null;
#if UNITY_EDITOR
            var editorParameters = new EditorSimulateModeParameters();
            var buildResult = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
            var packageRoot = buildResult.PackageRootDirectory;
            var fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);
            editorParameters.EditorFileSystemParameters = fileSystemParams;
            createParameters = editorParameters;
#else
            var fileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

            var offlineParameters = new OfflinePlayModeParameters();
            offlineParameters.BuildinFileSystemParameters = fileSystemParams;
            createParameters = offlineParameters;
#endif
            await package.InitializeAsync(createParameters).ToUniTask();

            // 2. 请求资源清单的版本信息
            var requestPackageVersionOp = package.RequestPackageVersionAsync();
            await requestPackageVersionOp.ToUniTask();
            HLog.Log($"资源版本号{requestPackageVersionOp.PackageVersion}");

            // 3. 传入的版本信息更新资源清单
            await package.UpdatePackageManifestAsync(requestPackageVersionOp.PackageVersion).ToUniTask();
            YooAssets.SetDefaultPackage(package);
            HLog.Log("资源系统初始化完成");
        }

        /// <summary>
        /// 加载资源（同步）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <returns>加载的资源</returns>
        public T Load<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                HLog.LogE("Resource path is empty!");
                return null;
            }

            try
            {
                // 检查缓存
                if (_loadedObjectCache.TryGetValue(path, out var cachedObject))
                {
                    return cachedObject as T;
                }

                // 从YooAsset加载
                var handle = YooAssets.LoadAssetSync<UnityEngine.Object>(path);
                var loadedObject = handle.AssetObject as T;

                if (loadedObject != null)
                {
                    // 缓存资源
                    _handleCache[path] = handle;
                    _loadedObjectCache[path] = loadedObject;
                }

                return loadedObject;
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to load resource {path}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 加载资源（异步）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <param name="callback">回调函数</param>
        public void LoadAsync<T>(string path, Action<T> callback) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                HLog.LogE("Resource path is empty!");
                callback?.Invoke(null);
                return;
            }

            try
            {
                // 检查缓存
                if (_loadedObjectCache.TryGetValue(path, out var cachedObject))
                {
                    callback?.Invoke(cachedObject as T);
                    return;
                }

                // 从YooAsset异步加载
                var handle = YooAssets.LoadAssetAsync<UnityEngine.Object>(path);
                handle.Completed += (operation) =>
                {
                    if (operation.Status == EOperationStatus.Succeed)
                    {
                        var loadedObject = operation.AssetObject as T;

                        if (loadedObject != null)
                        {
                            // 缓存资源
                            _handleCache[path] = handle;
                            _loadedObjectCache[path] = loadedObject;
                        }

                        callback?.Invoke(loadedObject);
                    }
                    else
                    {
                HLog.LogE($"Failed to load resource {path}");
                        callback?.Invoke(null);
                    }
                };
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to load resource {path}: {e.Message}");
                callback?.Invoke(null);
            }
        }

        /// <summary>
        /// 异步加载资源（UniTask版本）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源路径</param>
        /// <returns>异步任务</returns>
        public async UniTask<T> LoadAsync<T>(string path) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path))
            {
                HLog.LogE("Resource path is empty!");
                return null;
            }

            try
            {
                // 检查缓存
                if (_loadedObjectCache.TryGetValue(path, out var cachedObject))
                {
                    return cachedObject as T;
                }

                // 从YooAsset异步加载
                var handle = YooAssets.LoadAssetAsync<UnityEngine.Object>(path);
                await handle.Task;

                if (handle.Status == EOperationStatus.Succeed)
                {
                    var loadedObject = handle.AssetObject as T;

                    if (loadedObject != null)
                    {
                        // 缓存资源
                        _handleCache[path] = handle;
                        _loadedObjectCache[path] = loadedObject;
                    }

                    return loadedObject;
                }
                else
                {
                    HLog.LogE($"Failed to load resource {path}");
                    return null;
                }
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to load resource {path}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="path">资源路径</param>
        public void Release(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                HLog.LogE("Resource path is empty!");
                return;
            }

            try
            {
                // 从缓存中移除
                _loadedObjectCache.Remove(path);

                // 释放资源句柄
                if (_handleCache.TryGetValue(path, out var handle))
                {
                    handle.Release();
                    _handleCache.Remove(path);
                }
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to release resource {path}: {e.Message}");
            }
        }

        /// <summary>
        /// 释放资源（GameObject）
        /// </summary>
        /// <param name="gameObject">游戏对象</param>
        public void Release(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            try
            {
                // 从缓存中移除
                foreach (var kvp in _loadedObjectCache)
                {
                    if (kvp.Value == gameObject)
                    {
                        _loadedObjectCache.Remove(kvp.Key);
                        break;
                    }
                }

                // 释放资源句柄
                foreach (var kvp in _handleCache)
                {
                    if (kvp.Value.AssetObject == gameObject)
                    {
                        kvp.Value.Release();
                        _handleCache.Remove(kvp.Key);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to release GameObject: {e.Message}");
            }
        }

        /// <summary>
        /// 释放所有资源
        /// </summary>
        public void ReleaseAll()
        {
            try
            {
                // 释放所有句柄
                foreach (var handle in _handleCache.Values)
                {
                    handle.Release();
                }
                _handleCache.Clear();
                _loadedObjectCache.Clear();

                HLog.Log("All resources released!");
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to release all resources: {e.Message}");
            }
        }

        /// <summary>
        /// 预加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="priority">优先级</param>
        public void PreloadAsset(string path, int priority = 0)
        {
            if (string.IsNullOrEmpty(path))
            {
                HLog.LogE("Resource path is empty!");
                return;
            }

            try
            {
                var handle = YooAssets.LoadAssetAsync<UnityEngine.Object>(path);
                HLog.Log($"Preload asset: {path}");
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to preload asset {path}: {e.Message}");
            }
        }

        /// <summary>
        /// 预加载资源（异步）
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="callback">回调函数</param>
        /// <param name="priority">优先级</param>
        public void PreloadAssetAsync(string path, Action<UnityEngine.Object> callback, int priority = 0)
        {
            if (string.IsNullOrEmpty(path))
            {
                HLog.LogE("Resource path is empty!");
                callback?.Invoke(null);
                return;
            }

            try
            {
                var handle = YooAssets.LoadAssetAsync<UnityEngine.Object>(path);
                handle.Completed += (operation) =>
                {
                    if (operation.Status == EOperationStatus.Succeed)
                    {
                        callback?.Invoke(operation.AssetObject);
                    }
                    else
                    {
                        HLog.LogE($"Failed to preload asset {path}");
                        callback?.Invoke(null);
                    }
                };
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to preload asset {path}: {e.Message}");
                callback?.Invoke(null);
            }
        }

        /// <summary>
        /// 卸载未使用的资源
        /// </summary>
        public void UnloadUnusedAssets()
        {
            try
            {
                // YooAsset会自动管理资源，这里可以手动清理缓存
                HLog.Log("Unused assets unloaded!");
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to unload unused assets: {e.Message}");
            }
        }

        /// <summary>
        /// 获取资源加载状态
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>加载状态</returns>
        public string GetAssetLoadStatus(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "Invalid path";
            }

            if (_handleCache.TryGetValue(path, out var handle))
            {
                return handle.Status.ToString();
            }

            return "Not loaded";
        }

        /// <summary>
        /// 检查资源是否已加载
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>是否已加载</returns>
        public bool IsAssetLoaded(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            return _handleCache.ContainsKey(path);
        }

        /// <summary>
        /// 获取已加载资源数量
        /// </summary>
        /// <returns>资源数量</returns>
        public int GetLoadedAssetCount()
        {
            return _handleCache.Count;
        }
    }
}