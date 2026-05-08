using System.Collections.Generic;
using Animancer;
using GameAnimation.Core;
using GameAnimation.Data;
using UnityEngine;

namespace GameAnimation.Samples
{
    /// <summary>
    /// Creates a CharacterAnimationProfile at runtime with procedural animations for testing.
    /// </summary>
    public static class TestAnimationProfileFactory
    {
        public const int IdleAnimationId = 1001;
        public const int WalkAnimationId = 1002;
        public const int RunAnimationId = 1003;
        public const int AttackAnimationId = 2001;
        public const int JumpAnimationId = 2002;

        public static CharacterAnimationProfile CreateTestProfile()
        {
            var profile = ScriptableObject.CreateInstance<CharacterAnimationProfile>();
            profile.name = "TestAnimationProfile_Runtime";

            // Create layers
            var layers = new List<AnimationLayerDefinition>
            {
                new()
                {
                    layer = AnimationLayerType.Locomotion,
                    layerIndex = 0,
                    additive = false,
                    defaultWeight = 1f,
                    defaultFadeDuration = 0.15f
                },
                new()
                {
                    layer = AnimationLayerType.Action,
                    layerIndex = 1,
                    additive = false,
                    defaultWeight = 1f,
                    defaultFadeDuration = 0.1f
                }
            };

            // Create animations
            var animations = new List<AnimationDefinition>
            {
                CreateIdleDefinition(),
                CreateWalkDefinition(),
                CreateRunDefinition(),
                CreateAttackDefinition(),
                CreateJumpDefinition()
            };

            // Create locomotion with linear speed mixer
            var locomotion = new LocomotionDefinition
            {
                blendMode = LocomotionBlendMode.LinearSpeed,
                layer = AnimationLayerType.Locomotion,
                fadeDuration = 0.2f,
                parameterDampTime = 0.1f,
                rootMotionPolicy = AnimationRootMotionPolicy.Ignore,
                linearSpeedMixer = CreateLocomotionMixer()
            };

            // Use reflection to set private fields since they're serialized
            SetPrivateField(profile, "layers", layers);
            SetPrivateField(profile, "locomotion", locomotion);
            SetPrivateField(profile, "animations", animations);

            return profile;
        }

        private static AnimationDefinition CreateIdleDefinition()
        {
            return new AnimationDefinition
            {
                animationId = IdleAnimationId,
                displayName = "Idle",
                clip = TestAnimationFactory.CreateIdleClip(),
                layer = AnimationLayerType.Locomotion,
                priority = 0,
                canBeInterrupted = true,
                restartOnPlay = false,
                clearLayerOnComplete = false,
                fadeInDuration = 0.2f,
                fadeOutDuration = 0.2f
            };
        }

        private static AnimationDefinition CreateWalkDefinition()
        {
            return new AnimationDefinition
            {
                animationId = WalkAnimationId,
                displayName = "Walk",
                clip = TestAnimationFactory.CreateWalkClip(),
                layer = AnimationLayerType.Locomotion,
                priority = 0,
                canBeInterrupted = true,
                restartOnPlay = false,
                clearLayerOnComplete = false,
                fadeInDuration = 0.15f,
                fadeOutDuration = 0.15f
            };
        }

        private static AnimationDefinition CreateRunDefinition()
        {
            return new AnimationDefinition
            {
                animationId = RunAnimationId,
                displayName = "Run",
                clip = TestAnimationFactory.CreateRunClip(),
                layer = AnimationLayerType.Locomotion,
                priority = 0,
                canBeInterrupted = true,
                restartOnPlay = false,
                clearLayerOnComplete = false,
                fadeInDuration = 0.1f,
                fadeOutDuration = 0.1f
            };
        }

        private static AnimationDefinition CreateAttackDefinition()
        {
            return new AnimationDefinition
            {
                animationId = AttackAnimationId,
                displayName = "Attack",
                clip = TestAnimationFactory.CreateAttackClip(),
                layer = AnimationLayerType.Action,
                priority = 10,
                canBeInterrupted = false,
                restartOnPlay = true,
                clearLayerOnComplete = true,
                fadeInDuration = 0.08f,
                fadeOutDuration = 0.15f
            };
        }

        private static AnimationDefinition CreateJumpDefinition()
        {
            return new AnimationDefinition
            {
                animationId = JumpAnimationId,
                displayName = "Jump",
                clip = TestAnimationFactory.CreateJumpClip(),
                layer = AnimationLayerType.Action,
                priority = 15,
                canBeInterrupted = false,
                restartOnPlay = true,
                clearLayerOnComplete = true,
                fadeInDuration = 0.05f,
                fadeOutDuration = 0.1f
            };
        }

        private static LinearMixerTransition CreateLocomotionMixer()
        {
            var mixer = new LinearMixerTransition();

            var idleClip = TestAnimationFactory.CreateIdleClip();
            var walkClip = TestAnimationFactory.CreateWalkClip();
            var runClip = TestAnimationFactory.CreateRunClip();

            // Animations are Object[], can directly assign AnimationClip
            mixer.Animations = new UnityEngine.Object[]
            {
                idleClip,
                walkClip,
                runClip
            };

            // Speed thresholds: 0 (idle), 3 (walk), 6 (run)
            mixer.Thresholds = new float[] { 0f, 3f, 6f };

            return mixer;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }
    }
}