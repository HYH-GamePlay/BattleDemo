namespace GameAnimation.Core
{
    public enum AnimationPlayFailureReason
    {
        None,
        ActorNotInitialized,
        ProfileMissing,
        AnimationDefinitionMissing,
        AnimationClipMissing,
        AnimancerMissing,
        LayerBlocked,
    }
}
