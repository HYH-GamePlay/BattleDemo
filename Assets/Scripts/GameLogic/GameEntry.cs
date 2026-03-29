using System;
using GameCore;
using UnityEngine;

namespace GameLogic{
    public class GameEntry : MonoBehaviour{
        private void Awake(){
            ServiceLocator.Register<IAudioComp>(new AudioComp());
            ServiceLocator.Register<IEventComp>(new EventComp());
            ServiceLocator.Register<IResComp>(new ResComp());
            ServiceLocator.Register<ITickComp>(new TickComp());
            ServiceLocator.Register<IUIComp>(new UIComp());
            
            DontDestroyOnLoad(this);
        }
    }
}