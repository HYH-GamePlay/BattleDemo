using GameCore.Core;
using GameCore.Core.Comp.Event;
using GameCore.FSM;
using GameLogic.GameProcess.State;

namespace GameLogic.GameProcess{
    public class GameFsm : Fsm<GameEntry>{
        protected override void Init(){
            base.Init();
            
            Game.Event.AddListener<GameStateId>(EventId.ChangeGameState, OnChangeState);
        }

        private void OnChangeState(GameStateId state){
            foreach (var (key, fsmState) in StateDic){
                if (fsmState is GameStateBase gameStateBase && gameStateBase.stateId == state){
                    ChangeState(gameStateBase);
                    break;
                }
            }
        }
    }
}