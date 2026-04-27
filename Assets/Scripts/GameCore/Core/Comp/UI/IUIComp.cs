namespace GameCore.Core.Comp.UI{
    public interface IUIComp : IComp{
        public void OpenUI<T>(IUIData data = null) where T : UIPanelBase;
        public void CloseUI<T>() where T : UIPanelBase;
    }
}