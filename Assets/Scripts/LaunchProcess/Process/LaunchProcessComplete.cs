using System;
using GameCore;
using GameCore.FSM;
using UnityEngine.SceneManagement;

namespace LaunchProcess.Process{
    public class LaunchProcessComplete : FsmState<LaunchEntry>{
        public override void OnEnter(IFsm<LaunchEntry> fsm){
            base.OnEnter(fsm);
        }

        public override void OnExit(){
            base.OnExit();

            SceneManager.LoadScene("GameEntry");
        }

        public override void OnTick(TimeSpan ts){
            base.OnTick(ts);
            
        }
    }
}