using System;

namespace GameAnimation.Core
{
    [Serializable]
    public sealed class AnimationCommand
    {
        public int animationId;
        public bool hasLayerOverride;
        public AnimationLayerType layerOverride;
        public bool hasFadeDurationOverride;
        public float fadeDurationOverride;
        public bool hasSpeedOverride;
        public float speedOverride = 1f;
        public bool hasPriorityOverride;
        public int priorityOverride;
        public bool restart = true;
        public bool ignoreInterruption;
        public int requiredCancelGroupId;
        public float normalizedStartTime;
        public int sourceId;

        public static AnimationCommand Create(int animationId)
        {
            return new AnimationCommand
            {
                animationId = animationId,
                restart = true,
                speedOverride = 1f,
            };
        }
    }
}
