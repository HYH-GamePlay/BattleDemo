namespace Battle.CombatInfo.Tag
{
    public sealed class TagContainerInfo : ICombatInfo
    {
        public TagSet Tags { get; } = new TagSet();
    }
}
