namespace GameCore{
    public static class Game{
        public static IEventComp Event => ServiceLocator.Get<IEventComp>();
        public static IResComp Resource => ServiceLocator.Get<IResComp>();
        public static IAudioComp Audio => ServiceLocator.Get<IAudioComp>();
        public static IUIComp UI => ServiceLocator.Get<IUIComp>();
        public static ITickComp Tick => ServiceLocator.Get<ITickComp>();
    }
}