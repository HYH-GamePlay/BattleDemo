using Cysharp.Threading.Tasks;
using Tools.Log;
using UnityEngine;

namespace GameCore.Core.Comp.UI{
    /// <summary>
    /// UI面板基类
    /// 提供面板的生命周期管理、动画流程和交互功能
    /// </summary>
    public abstract class UIPanelBase : UIBase{
        private UIDataBase _data;

        /// <summary>
        /// UI面板数据
        /// </summary>
        public UIDataBase Data => _data;

        /// <summary>
        /// 初始化UI面板
        /// </summary>
        public override void Init()
        {
            base.Init();
            OnInit();
        }

        /// <summary>
        /// 初始化UI面板（子类实现）
        /// </summary>
        protected virtual void OnInit()
        {
            // 子类实现
        }

        /// <summary>
        /// 打开UI面板
        /// </summary>
        /// <param name="data">UI数据</param>
        public override void Open(IUIData data = null)
        {
            base.Open(data);

            if (data != null)
            {
                _data = data as UIDataBase;
                if (_data == null)
                {
                HLog.LogW($"UI {UIName} data type mismatch!");
                }
            }

            OnOpen(data);
        }

        /// <summary>
        /// 打开UI面板（子类实现）
        /// </summary>
        /// <param name="data">UI数据</param>
        protected virtual void OnOpen(IUIData data)
        {
            // 子类实现
        }

        /// <summary>
        /// 关闭UI面板
        /// </summary>
        public override void Close()
        {
            base.Close();
            OnClose();
        }

        /// <summary>
        /// 关闭UI面板（子类实现）
        /// </summary>
        protected virtual void OnClose()
        {
            // 子类实现
        }

        /// <summary>
        /// 关闭UI面板动画
        /// </summary>
        /// <returns>异步任务</returns>
        protected virtual UniTask OnCloseAnima()
        {
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 关闭UI面板结束（子类实现）
        /// </summary>
        protected virtual void OnCloseEnd()
        {
            // 子类实现
        }

        /// <summary>
        /// 反初始化UI面板
        /// </summary>
        public override void UnInit()
        {
            base.UnInit();
            OnUnInit();
        }

        /// <summary>
        /// 反初始化UI面板（子类实现）
        /// </summary>
        protected virtual void OnUnInit()
        {
            // 子类实现
        }

        /// <summary>
        /// 销毁UI面板
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();
            OnDestroy();
        }

        /// <summary>
        /// 销毁UI面板（子类实现）
        /// </summary>
        protected virtual void OnDestroy()
        {
            // 子类实现
        }
    }
}