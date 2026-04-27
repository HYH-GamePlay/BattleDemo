using GameCore.Core;
using GameCore.Core.Comp;
using GameCore.FSM;
using Tools.Log;

namespace GameLogic.GameProcess.State{
    public class GameStateInit : GameStateBase{
        public override GameStateId stateId => GameStateId.Init;

        public override void OnEnter(IFsm<GameEntry> fsm){
            base.OnEnter(fsm);
            HLog.Log("进入:" + stateId);
            
            ChangeState<GameStateBattle>();
        }
    }
}