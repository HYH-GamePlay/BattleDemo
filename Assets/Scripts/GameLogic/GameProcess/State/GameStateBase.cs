using GameCore.FSM;

namespace GameLogic.GameProcess.State{
    public abstract class GameStateBase : FsmState<GameEntry>{
        public abstract GameStateId stateId{ get; }
    }
}