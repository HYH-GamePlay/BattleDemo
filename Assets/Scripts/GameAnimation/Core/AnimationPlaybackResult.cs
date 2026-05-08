namespace GameAnimation.Core
{
    public readonly struct AnimationPlaybackResult
    {
        public bool Accepted { get; }
        public int AnimationId { get; }
        public AnimationPlayFailureReason FailureReason { get; }

        private AnimationPlaybackResult(bool accepted, int animationId, AnimationPlayFailureReason failureReason)
        {
            Accepted = accepted;
            AnimationId = animationId;
            FailureReason = failureReason;
        }

        public static AnimationPlaybackResult AcceptedResult(int animationId)
        {
            return new AnimationPlaybackResult(true, animationId, AnimationPlayFailureReason.None);
        }

        public static AnimationPlaybackResult RejectedResult(int animationId, AnimationPlayFailureReason failureReason)
        {
            return new AnimationPlaybackResult(false, animationId, failureReason);
        }
    }
}
