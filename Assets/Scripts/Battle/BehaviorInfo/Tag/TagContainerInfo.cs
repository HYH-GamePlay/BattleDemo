namespace Battle.CombatInfo.Tag
{
    public sealed class TagContainerInfo : IBehaviorInfo
    {
        public TagSet Tags { get; } = new TagSet();
    }
}
