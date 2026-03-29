namespace GameCore.FSM{
    public interface IFsmState{
        protected void OnEnter();
        protected void OnUpdate(float deltaTime);
        protected void OnExit();
        protected void OnDestroy();
    }
}