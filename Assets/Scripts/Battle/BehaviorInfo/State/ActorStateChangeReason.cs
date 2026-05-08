namespace Battle.CombatInfo.State
{
    public enum ActorStateChangeReason : byte
    {
        None = 0,
        Initialize = 1,
        Request = 2,
        ActionStarted = 3,
        ActionFinished = 4,
        Interrupted = 5,
        Hit = 6,
        Dead = 7,
    }
}
