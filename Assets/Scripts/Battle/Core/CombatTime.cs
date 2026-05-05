namespace Battle.Core
{
    public readonly struct CombatTime
    {
        public CombatTime(long frame, float deltaTime, float elapsedTime)
        {
            Frame = frame;
            DeltaTime = deltaTime;
            ElapsedTime = elapsedTime;
        }

        public long Frame { get; }

        public float DeltaTime { get; }

        public float ElapsedTime { get; }
    }
}
