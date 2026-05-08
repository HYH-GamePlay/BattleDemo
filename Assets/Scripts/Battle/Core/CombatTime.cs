using System;

namespace Battle.Core
{
    public readonly struct CombatTime
    {
        public CombatTime(long frame, TimeSpan ts, TimeSpan elapsedTime)
        {
            Frame = frame;
            DeltaTime = ts;
            ElapsedTime = elapsedTime;
        }

        public long Frame { get; }

        public TimeSpan DeltaTime { get; }

        public TimeSpan ElapsedTime { get; }
    }
}
