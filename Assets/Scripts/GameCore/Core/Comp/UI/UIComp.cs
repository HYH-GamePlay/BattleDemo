using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore.Core.Tools;
using Tools.Log;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameCore.Core.Comp.UI{
    /// <summary>
    /// UI组件
    /// 管理所有UI面板，处理UI打开和关闭逻辑
    /// </summary>
    public class UIComp : IUIComp{
        private Dictionary<Type, UIPanelBase> _uiPanelDic = new();
        private Dictionary<Type, GameObject> _uiPanelObjectDic = new();
        private GameObject _uiRoot;
        
        private const float LayerDistance = 1000;

        /// <summary>
        /// 初始化UI组件
        /// </summary>
        public UniTask Init()
        {
            if (_uiRoot != null)
            {
                HLog.LogW("UIComp already initialized!");
                return UniTask.CompletedTask;
            }

            // 查找UIRoot
            _uiRoot = GameObject.Find("UIRoot");
            if (_uiRoot == null)
            {
                HLog.LogE("UIRoot not found!");
                return UniTask.CompletedTask;
            }

            HLog.Log("UIComp initialized successfully!");
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 打开UI面板
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <param name="data">UI数据</param>
        public void OpenUI<T>(IUIData data = null) where T : UIPanelBase
        {
            Type panelType = typeof(T);

            // 检查是否已经打开
            if (_uiPanelDic.ContainsKey(panelType))
            {
                HLog.LogW($"UI {panelType.Name} already opened!");
                return;
            }

            // 从配置加载UI
            if (UIGlobalConfig.Instance.UIConfigs.TryGetValue(panelType, out var uiConfig) && uiConfig.prefab.Asset)
            {
                // 创建UI对象
                GameObject uiObject = Object.Instantiate(uiConfig.prefab.Asset, _uiRoot.transform, true) as GameObject;
                uiObject?.SetActive(true);

                if (uiObject == null)
                {
                    HLog.LogE($"Failed to instantiate UI {panelType.Name}!");
                    return;
                }
                
                var rect = uiObject.GetComponent<RectTransform>();
                rect.localScale = Vector3.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchorMax = Vector2.one;
                rect.anchorMin = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
                rect.localRotation = Quaternion.identity;
                rect.localPosition = new Vector3(0, 0, (int)uiConfig.layer * LayerDistance); //这个是为了3D模型时穿插的问题

                // 添加UI面板组件
                UIPanelBase panel = uiObject.GetOrAddComp(uiConfig.script.ScriptType) as UIPanelBase;
                if (panel == null)
                {
                    HLog.LogE($"Failed to get UI panel component {panelType.Name}!");
                    Object.Destroy(uiObject);
                    return;
                }

                // 初始化UI面板
                panel.Init();

                // 打开UI面板
                panel.Open(data);

                // 添加到字典
                _uiPanelDic.Add(panelType, panel);
                _uiPanelObjectDic.Add(panelType, uiObject);
            }
            else
            {
                HLog.LogE($"UI config not found for type {panelType.Name}!");
            }
        }

        /// <summary>
        /// 关闭UI面板
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        public void CloseUI<T>() where T : UIPanelBase
        {
            CloseUI(typeof(T));
        }

        /// <summary>
        /// 关闭UI面板
        /// </summary>
        /// <param name="panelType">UI面板类型</param>
        public void CloseUI(Type panelType)
        {
            // 检查是否已打开
            if (!_uiPanelDic.ContainsKey(panelType))
            {
                HLog.LogW($"UI {panelType.Name} not opened!");
                return;
            }

            // 获取UI面板
            if (_uiPanelDic.TryGetValue(panelType, out UIPanelBase panel))
            {
                // 关闭UI面板
                panel.Close();

                // 从字典中移除
                _uiPanelDic.Remove(panelType);
                if (_uiPanelObjectDic.TryGetValue(panelType, out GameObject uiObject))
                {
                    _uiPanelObjectDic.Remove(panelType);
                    // 销毁UI对象
                    Object.Destroy(uiObject);
                }
            }
        }

        /// <summary>
        /// 关闭所有UI面板
        /// </summary>
        public void CloseAllUI()
        {
            // 反向遍历，避免索引问题
            var panelTypes = new List<Type>(_uiPanelDic.Keys);
            foreach (var panelType in panelTypes)
            {
                CloseUI(panelType);
            }
        }

        /// <summary>
        /// 获取UI面板
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <returns>UI面板实例</returns>
        public T GetUI<T>() where T : UIPanelBase
        {
            if (_uiPanelDic.TryGetValue(typeof(T), out UIPanelBase panel))
            {
                return panel as T;
            }
            return null;
        }

        /// <summary>
        /// 检查UI面板是否已打开
        /// </summary>
        /// <typeparam name="T">UI面板类型</typeparam>
        /// <returns>是否已打开</returns>
        public bool IsUIOpened<T>() where T : UIPanelBase
        {
            return _uiPanelDic.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 反初始化UI组件
        /// </summary>
        public UniTask UnInit()
        {
            // 关闭所有UI面板
            CloseAllUI();

            _uiRoot = null;
            _uiPanelDic.Clear();
            _uiPanelObjectDic.Clear();

            HLog.Log("UIComp uninitialized successfully!");
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 获取UI面板数量
        /// </summary>
        /// <returns>UI面板数量</returns>
        public int GetUICount()
        {
            return _uiPanelDic.Count;
        }
    }
}