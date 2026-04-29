namespace Combat.Runtime.Effects
{
    public readonly struct EffectApplicationRequest
    {
        public readonly string SourceActorIdentifier;
        public readonly string TargetActorIdentifier;
        public readonly string EffectIdentifier;

        public EffectApplicationRequest(string sourceActorIdentifier, string targetActorIdentifier, string effectIdentifier)
        {
            SourceActorIdentifier = sourceActorIdentifier;
            TargetActorIdentifier = targetActorIdentifier;
            EffectIdentifier = effectIdentifier;
        }
    }
}
