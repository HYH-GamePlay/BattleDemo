using Cysharp.Threading.Tasks;
using Tools.Log;
using UnityEngine.EventSystems;
using UnityEngine;

namespace GameCore.Core.Comp.UI{
    /// <summary>
    /// UI基础类
    /// 提供UI的基础功能
    /// </summary>
    public abstract class UIBase : UIBehaviour
    {
        /// <summary>
        /// UI面板名称
        /// </summary>
        public string UIName { get; protected set; }

        /// <summary>
        /// UI是否已初始化
        /// </summary>
        protected bool IsInitialized { get; set; } = false;

        /// <summary>
        /// UI是否已打开
        /// </summary>
        protected bool IsOpened { get; set; } = false;

        /// <summary>
        /// UI是否正在关闭
        /// </summary>
        protected bool IsClosing { get; set; } = false;

        /// <summary>
        /// UI是否已销毁
        /// </summary>
        protected bool IsDestroyed { get; set; } = false;

        /// <summary>
        /// 初始化UI
        /// </summary>
        public virtual void Init()
        {
            if (IsInitialized)
            {
                HLog.LogW($"UI {UIName} already initialized!");
                return;
            }

            IsInitialized = true;
            OnInit();
        }

        /// <summary>
        /// 初始化UI（子类实现）
        /// </summary>
        protected virtual void OnInit()
        {
            // 子类实现
        }

        /// <summary>
        /// 打开UI
        /// </summary>
        /// <param name="data">UI数据</param>
        public virtual void Open(IUIData data = null)
        {
            if (!IsInitialized)
            {
                HLog.LogE($"UI {UIName} not initialized!");
                return;
            }

            if (IsOpened)
            {
                HLog.LogW($"UI {UIName} already opened!");
                return;
            }

            if (IsClosing)
            {
                HLog.LogW($"UI {UIName} is closing!");
                return;
            }

            IsOpened = true;
            OnOpen(data);
        }

        /// <summary>
        /// 打开UI（子类实现）
        /// </summary>
        /// <param name="data">UI数据</param>
        protected virtual void OnOpen(IUIData data)
        {
            // 子类实现
        }

        /// <summary>
        /// 关闭UI
        /// </summary>
        public virtual void Close()
        {
            if (!IsOpened)
            {
                HLog.LogW($"UI {UIName} not opened!");
                return;
            }

            if (IsClosing)
            {
                HLog.LogW($"UI {UIName} is closing!");
                return;
            }

            IsClosing = true;
            OnClose();

            // 等待关闭动画完成后销毁
            UniTask.RunOnThreadPool(async () =>
            {
                await OnCloseAnima();
                OnCloseEnd();
                IsClosing = false;
                IsOpened = false;

                // 延迟销毁，确保动画播放完成
                await UniTask.Delay(100);
                Destroy(gameObject);
            }).Forget();
        }

        /// <summary>
        /// 关闭UI（子类实现）
        /// </summary>
        protected virtual void OnClose()
        {
            // 子类实现
        }

        /// <summary>
        /// 关闭UI动画
        /// </summary>
        /// <returns>异步任务</returns>
        protected virtual UniTask OnCloseAnima()
        {
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 关闭UI结束（子类实现）
        /// </summary>
        protected virtual void OnCloseEnd()
        {
            // 子类实现
        }

        /// <summary>
        /// 反初始化UI
        /// </summary>
        public virtual void UnInit()
        {
            if (IsOpened)
            {
                Close();
                return;
            }

            if (IsInitialized)
            {
                IsInitialized = false;
                OnUnInit();
            }
        }

        /// <summary>
        /// 反初始化UI（子类实现）
        /// </summary>
        protected virtual void OnUnInit()
        {
            // 子类实现
        }

        /// <summary>
        /// 销毁UI
        /// </summary>
        public virtual void Destroy()
        {
            if (IsDestroyed)
            {
                return;
            }

            IsDestroyed = true;
            OnDestroy();
            Destroy(gameObject);
        }

        /// <summary>
        /// 销毁UI（子类实现）
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 子类实现
        }

        /// <summary>
        /// 设置UI名称
        /// </summary>
        /// <param name="name">UI名称</param>
        protected void SetUIName(string name)
        {
            UIName = name;
        }
    }
}