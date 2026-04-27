using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameCore;
using GameCore.Core;
using GameCore.Core.Comp;
using GameCore.Core.Comp.Config;
using GameCore.Core.Comp.Audio;
using GameCore.Core.Comp.Event;
using GameCore.Core.Comp.Res;
using GameCore.Core.Comp.Tick;
using GameCore.Core.Comp.UI;
using GameCore.FSM;
using GameLogic.GameProcess.State;
using UnityEngine;

namespace GameLogic{
    public class GameEntry : MonoBehaviour{
        private Fsm<GameEntry> _gameStateFsm;
        
        private void Awake(){
            DontDestroyOnLoad(this);
            
            Init().Forget();
            
            // Game.Event.AddListener<GameStateId>((int)EventId.ChangeGameState, OnChangeGameState);
        }

        private void OnDestroy(){
            UnInit().Forget();
        }

        private async UniTaskVoid Init(){
            ServiceLocator.Register<ITickComp>(new TickComp());
            ServiceLocator.Register<IEventComp>(new EventComp());
            ServiceLocator.Register<IResComp>(new ResComp());
            ServiceLocator.Register<IConfigComp>(new ConfigComp());
            ServiceLocator.Register<IAudioComp>(new AudioComp());
            ServiceLocator.Register<IUIComp>(new UIComp());
            
            _gameStateFsm = Fsm<GameEntry>.Creat("GameStateFsm", this, new List<FsmState<GameEntry>>{
                new GameStateInit(),
                new GameStateBattle()
            });
            
            foreach (var service in ServiceLocator.GetAll<IComp>()){
                await service.Init();
            }
            
            _gameStateFsm.Start<GameStateInit>();
        }

        private async UniTaskVoid UnInit(){
            foreach (var service in ServiceLocator.GetAll<IComp>()){
                await service.UnInit();
            }
        }

        private void Update(){
            Game.Tick.Tick(TimeSpan.FromSeconds(Time.deltaTime));
        }
        
        private void OnChangeGameState(GameStateId state){
        }
    }
}