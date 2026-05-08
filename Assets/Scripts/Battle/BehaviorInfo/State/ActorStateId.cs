namespace Battle.CombatInfo.State
{
    public enum ActorStateId : ushort
    {
        None = 0,
        Locomotion = 100,
        Attack = 200,
        Dodge = 300,
        HitStun = 400,
        Airborne = 500,
        Dead = 900,
    }
}
