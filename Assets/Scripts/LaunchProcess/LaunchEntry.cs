using System.Collections.Generic;
using GameCore.FSM;
using LaunchProcess.Process;
using Tools.Log;
using UnityEngine;
using YooAsset;

namespace LaunchProcess{
    /// <summary>
    /// 启动入口
    /// </summary>
    public class LaunchEntry : MonoBehaviour
    {
        [SerializeField]
        private EPlayMode playMode = EPlayMode.EditorSimulateMode;
    
        private Fsm<LaunchEntry> _fsm;
    
        private void Awake(){
            Init();
        }

        private void Init(){
            _fsm = Fsm<LaunchEntry>.Creat("GameEntry", this, new List<FsmState<LaunchEntry>>(){
                new LaunchProcessCheckVersion(),
                new LaunchProcessDownloadManifest(),
                new LaunchProcessDownloadRes(),
                new LaunchProcessVerifyFiles(),
                new LaunchProcessError(),
                new LaunchProcessComplete(),
            });
            
            Launch();
        }

        public void Launch(){
            HLog.Log("启动更新");
            _fsm?.Start<LaunchProcessComplete>();
        }
    }
}
