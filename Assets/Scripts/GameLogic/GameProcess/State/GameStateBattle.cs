using GameCore.Core;
using GameCore.FSM;
using Tools.Log;
using YooAsset;

namespace GameLogic.GameProcess.State{
    public class GameStateBattle : GameStateBase{
        private const string BattleScenePath = "Assets/Res/Scenes/BattleTest.unity";

        public override GameStateId stateId => GameStateId.Battle;

        public override void OnEnter(IFsm<GameEntry> fsm){
            base.OnEnter(fsm);

            HLog.Log("进入:" + stateId);

            // 加载战斗测试场景
            LoadBattleScene();
        }

        public override void OnExit(){
            base.OnExit();

            // 卸载战斗场景
            UnloadBattleScene();
        }

        private async void LoadBattleScene(){
            // 使用YooAsset加载场景
            var handle = YooAssets.LoadSceneAsync(BattleScenePath);
            await handle.Task;

            if (handle.Status == YooAsset.EOperationStatus.Succeed){
                HLog.Log("战斗场景加载成功");
            }
            else{
                HLog.LogE("战斗场景加载失败");
            }
        }

        private void UnloadBattleScene(){
            // 场景会在状态切换时自动卸载
        }
    }
}