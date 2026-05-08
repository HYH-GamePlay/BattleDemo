namespace Battle.CombatInfo.Action
{
    public enum InstructType : ushort
    {
        None = 0,

        Animation = 1,
        AddTag = 2,
        RemoveTag = 3,
        Collision = 4,
        ApplyEffect = 5,
        Motion = 6,
        CreateBullet = 7,
        Audio = 8,
        Camera = 9,
        Vfx = 10,
        ActionLink = 11,
    }
}
