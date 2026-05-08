using System;
using Battle.CombatInfo.Action;
using Battle.Core;
using GameCore.Core;
using GameCore.Core.Comp;
using GameCore.FSM;
using Tools.Log;

namespace GameLogic.GameProcess.State{
    public class GameStateInit : GameStateBase{
        public override GameStateId stateId => GameStateId.Init;
        
        private CombatWorld _combatWorld;

        public override void OnEnter(IFsm<GameEntry> fsm){
            base.OnEnter(fsm);

            HLog.Log("初始化游戏");
            _combatWorld = new CombatWorld();
        }

        public override void OnTick(TimeSpan ts)
        {
            base.OnTick(ts);
            
            _combatWorld.Tick(ts);
        }
    }
}
