namespace GameCore{
    public interface IUIComp : IComp{
        public void OpenUI(int id);
        public void CloseUI(int id);
    }
}