namespace Battle.Core
{
    public enum CombatPhase
    {
        PreUpdate = 0,
        Input = 100,
        Ability = 200,
        Movement = 300,
        Hit = 400,
        Damage = 500,
        State = 600,
        Presentation = 700,
        PostUpdate = 800,
    }
}
