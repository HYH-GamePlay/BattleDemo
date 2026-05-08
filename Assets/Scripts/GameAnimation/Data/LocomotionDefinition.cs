using System;
using Animancer;
using GameAnimation.Core;

namespace GameAnimation.Data
{
    [Serializable]
    public sealed class LocomotionDefinition
    {
        public LocomotionBlendMode blendMode = LocomotionBlendMode.LinearSpeed;
        public AnimationLayerType layer = AnimationLayerType.Locomotion;
        public int fallbackAnimationId;
        public float fadeDuration = 0.15f;
        public float parameterDampTime = 0.08f;
        public AnimationRootMotionPolicy rootMotionPolicy = AnimationRootMotionPolicy.Ignore;
        public LinearMixerTransition linearSpeedMixer = new();
        public MixerTransition2D directionalMixer = new();
    }
}
