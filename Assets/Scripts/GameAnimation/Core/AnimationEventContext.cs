namespace GameAnimation.Core
{
    [System.Serializable]
    public sealed class AnimationEventContext
    {
        public int ActorId { get; }
        public int AnimationId { get; }
        public int EventId { get; }
        public int PayloadId { get; }
        public AnimationEventType EventType { get; }
        public AnimationEventOrigin Origin { get; }
        public AnimationLayerType Layer { get; }
        public float NormalizedTime { get; }

        public AnimationEventContext(
            int actorId,
            int animationId,
            int eventId,
            int payloadId,
            AnimationEventType eventType,
            AnimationEventOrigin origin,
            AnimationLayerType layer,
            float normalizedTime)
        {
            ActorId = actorId;
            AnimationId = animationId;
            EventId = eventId;
            PayloadId = payloadId;
            EventType = eventType;
            Origin = origin;
            Layer = layer;
            NormalizedTime = normalizedTime;
        }
    }
}
