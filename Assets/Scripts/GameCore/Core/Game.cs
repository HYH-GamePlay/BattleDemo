using GameCore.Core.Comp.Audio;
using GameCore.Core.Comp.Config;
using GameCore.Core.Comp.Event;
using GameCore.Core.Comp.Res;
using GameCore.Core.Comp.Tick;
using GameCore.Core.Comp.UI;

namespace GameCore.Core{
    public static class Game{
        public static IEventComp Event => ServiceLocator.Get<IEventComp>();
        public static IResComp Resource => ServiceLocator.Get<IResComp>();
        public static IAudioComp Audio => ServiceLocator.Get<IAudioComp>();
        public static IUIComp UI => ServiceLocator.Get<IUIComp>();
        public static ITickComp Tick => ServiceLocator.Get<ITickComp>();
        public static IConfigComp Config => ServiceLocator.Get<IConfigComp>();
    }
}