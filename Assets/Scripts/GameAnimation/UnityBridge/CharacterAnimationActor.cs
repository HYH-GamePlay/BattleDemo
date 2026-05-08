using System;
using System.Collections.Generic;
using Animancer;
using GameAnimation.Core;
using GameAnimation.Data;
using GameAnimation.Runtime;
using UnityEngine;
using UnityEngine.Events;

namespace GameAnimation.UnityBridge
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AnimancerComponent))]
    public sealed class CharacterAnimationActor : MonoBehaviour
    {
        [SerializeField] private int actorId;
        [SerializeField] private AnimancerComponent animancer;
        [SerializeField] private CharacterAnimationProfile profile;
        [SerializeField] private RootMotionDriver rootMotionDriver;
        [SerializeField] private bool initializeOnAwake = true;
        [SerializeField] private bool startLocomotionOnInitialize = true;
        [SerializeField] private AnimationEventUnityEvent onAnimationEvent = new();

        private readonly Dictionary<AnimationLayerType, AnimationLayerPlaybackState> layerStates = new();
        private readonly AnimationEventScheduler eventScheduler = new();
        private AnimancerState locomotionState;
        private LinearMixerState linearLocomotionState;
        private Vector2MixerState directionalLocomotionState;
        private AnimationLocomotionInput locomotionInput;
        private float smoothedSpeed;
        private Vector2 smoothedLocalVelocity;
        private bool initialized;

        public int ActorId => actorId;
        public CharacterAnimationProfile Profile => profile;
        public event Action<AnimationEventContext> AnimationEventTriggered;

        private void Reset()
        {
            animancer = GetComponent<AnimancerComponent>();
            rootMotionDriver = GetComponent<RootMotionDriver>();
        }

        private void Awake()
        {
            if (initializeOnAwake)
                Initialize();
        }

        private void Update()
        {
            if (!initialized) return;

            UpdateLocomotion(Time.deltaTime);
            eventScheduler.Tick(DispatchAnimationEvent);
            ClearCompletedLayers();
        }

        public void BindActorId(int runtimeActorId)
        {
            actorId = runtimeActorId;
        }

        public bool Initialize()
        {
            ResolveReferences();
            if (animancer == null || profile == null)
                return false;

            ConfigureLayers();
            initialized = true;

            if (startLocomotionOnInitialize)
                StartLocomotion();

            return true;
        }

        public void SetProfile(CharacterAnimationProfile animationProfile, bool restartLocomotion)
        {
            profile = animationProfile;
            initialized = false;
            ClearLocomotionState();

            var previousStartLocomotionOnInitialize = startLocomotionOnInitialize;
            startLocomotionOnInitialize = restartLocomotion;
            try
            {
                Initialize();
            }
            finally
            {
                startLocomotionOnInitialize = previousStartLocomotionOnInitialize;
            }
        }

        public AnimationPlaybackResult PlayAnimation(int animationId)
        {
            return PlayAnimation(AnimationCommand.Create(animationId));
        }

        public AnimationPlaybackResult PlayAnimation(AnimationCommand command)
        {
            if (command == null)
                return AnimationPlaybackResult.RejectedResult(0, AnimationPlayFailureReason.AnimationDefinitionMissing);

            if (!initialized && !Initialize())
                return AnimationPlaybackResult.RejectedResult(command.animationId, AnimationPlayFailureReason.ActorNotInitialized);

            if (profile == null)
                return AnimationPlaybackResult.RejectedResult(command.animationId, AnimationPlayFailureReason.ProfileMissing);

            if (!profile.TryGetAnimationDefinition(command.animationId, out var definition))
                return AnimationPlaybackResult.RejectedResult(command.animationId, AnimationPlayFailureReason.AnimationDefinitionMissing);

            if (definition.clip == null)
                return AnimationPlaybackResult.RejectedResult(command.animationId, AnimationPlayFailureReason.AnimationClipMissing);

            if (animancer == null)
                return AnimationPlaybackResult.RejectedResult(command.animationId, AnimationPlayFailureReason.AnimancerMissing);

            var layerType = command.hasLayerOverride ? command.layerOverride : definition.layer;
            var layerState = GetOrCreateLayerState(layerType);
            if (layerState.CurrentDefinition == definition &&
                layerState.IsPlaying &&
                !command.restart &&
                !definition.restartOnPlay)
            {
                return AnimationPlaybackResult.AcceptedResult(definition.animationId);
            }

            if (!layerState.CanPlay(definition, command))
                return AnimationPlaybackResult.RejectedResult(command.animationId, AnimationPlayFailureReason.LayerBlocked);

            var layer = animancer.Layers[layerState.LayerIndex];
            var fadeDuration = ResolveFadeDuration(definition, command, layerType);
            var state = layer.Play(definition.clip, fadeDuration, FadeMode.FixedDuration);
            state.Speed = ResolveSpeed(definition, command);

            if (command.restart || definition.restartOnPlay || command.normalizedStartTime > 0f)
                state.NormalizedTime = Mathf.Clamp01(command.normalizedStartTime);

            layerState.SetCurrent(definition, state, command);
            eventScheduler.Schedule(actorId, definition, state, layerType);
            ApplyRootMotionPolicy(definition.rootMotionPolicy);

            return AnimationPlaybackResult.AcceptedResult(definition.animationId);
        }

        public bool StartLocomotion()
        {
            if (!initialized && !Initialize())
                return false;

            var locomotion = profile != null ? profile.Locomotion : null;
            if (locomotion == null || locomotion.blendMode == LocomotionBlendMode.Disabled)
                return false;

            ClearLocomotionState();

            var layerState = GetOrCreateLayerState(locomotion.layer);
            var layer = animancer.Layers[layerState.LayerIndex];
            var fadeDuration = locomotion.fadeDuration > 0f
                ? locomotion.fadeDuration
                : profile.GetDefaultFadeDuration(locomotion.layer);

            switch (locomotion.blendMode)
            {
                case LocomotionBlendMode.LinearSpeed:
                    if (locomotion.linearSpeedMixer == null || !locomotion.linearSpeedMixer.IsValid)
                        return PlayFallbackLocomotion(locomotion);
                    locomotionState = layer.Play(locomotion.linearSpeedMixer, fadeDuration, FadeMode.FixedDuration);
                    linearLocomotionState = locomotionState as LinearMixerState;
                    break;
                case LocomotionBlendMode.Directional2D:
                    if (locomotion.directionalMixer == null || !locomotion.directionalMixer.IsValid)
                        return PlayFallbackLocomotion(locomotion);
                    locomotionState = layer.Play(locomotion.directionalMixer, fadeDuration, FadeMode.FixedDuration);
                    directionalLocomotionState = locomotionState as Vector2MixerState;
                    break;
                case LocomotionBlendMode.FallbackAnimation:
                    return PlayFallbackLocomotion(locomotion);
                default:
                    return false;
            }

            ApplyRootMotionPolicy(locomotion.rootMotionPolicy);
            return locomotionState != null;
        }

        public void SetLocomotionInput(AnimationLocomotionInput input)
        {
            locomotionInput = input;
            if (locomotionState == null || !locomotionState.IsPlaying)
                StartLocomotion();
        }

        public void StopLayer(AnimationLayerType layerType, float fadeDuration = -1f)
        {
            if (!initialized && !Initialize())
                return;

            var layerState = GetOrCreateLayerState(layerType);
            var layer = animancer.Layers[layerState.LayerIndex];
            var resolvedFadeDuration = fadeDuration >= 0f
                ? fadeDuration
                : profile.GetDefaultFadeDuration(layerType);

            layer.StartFade(0f, resolvedFadeDuration);
            layerState.Clear();
            eventScheduler.ClearLayer(layerType);

            if (layerType == AnimationLayerType.Locomotion)
                ClearLocomotionState();
        }

        public bool IsInCancelWindow(AnimationLayerType layerType, int cancelGroupId)
        {
            return layerStates.TryGetValue(layerType, out var layerState) &&
                   layerState.IsInCancelWindow(cancelGroupId);
        }

        public RootMotionDelta ConsumeRootMotion()
        {
            return rootMotionDriver != null
                ? rootMotionDriver.ConsumeRootMotion()
                : new RootMotionDelta(Vector3.zero, Quaternion.identity);
        }

        public void DispatchUnityAnimationEvent(int eventId, int payloadId, AnimationEventType eventType)
        {
            var layerState = ResolvePrimaryEventLayer();
            DispatchAnimationEvent(new AnimationEventContext(
                actorId,
                layerState != null && layerState.CurrentDefinition != null
                    ? layerState.CurrentDefinition.animationId
                    : 0,
                eventId,
                payloadId,
                eventType,
                AnimationEventOrigin.UnityAnimationEvent,
                layerState != null ? layerState.Layer : AnimationLayerType.Base,
                layerState != null && layerState.CurrentState != null
                    ? layerState.CurrentState.NormalizedTime
                    : 0f));
        }

        private void ResolveReferences()
        {
            if (animancer == null)
                animancer = GetComponent<AnimancerComponent>();
            if (rootMotionDriver == null)
                rootMotionDriver = GetComponent<RootMotionDriver>();
        }

        private void ConfigureLayers()
        {
            layerStates.Clear();

            var maxLayerIndex = 0;
            foreach (var layerDefinition in profile.Layers)
            {
                if (layerDefinition == null) continue;
                if (layerDefinition.layerIndex > maxLayerIndex)
                    maxLayerIndex = layerDefinition.layerIndex;
            }

            animancer.Layers.SetMinCount(maxLayerIndex + 1);

            foreach (var layerDefinition in profile.Layers)
            {
                if (layerDefinition == null) continue;

                var layer = animancer.Layers[layerDefinition.layerIndex];
                layer.Mask = layerDefinition.avatarMask;
                layer.IsAdditive = layerDefinition.additive;
                layer.Weight = layerDefinition.defaultWeight;
                layerStates[layerDefinition.layer] = new AnimationLayerPlaybackState(
                    layerDefinition.layer,
                    layerDefinition.layerIndex);
            }
        }

        private AnimationLayerPlaybackState GetOrCreateLayerState(AnimationLayerType layerType)
        {
            if (layerStates.TryGetValue(layerType, out var layerState))
                return layerState;

            var layerIndex = profile != null ? profile.GetLayerIndex(layerType) : (int)layerType;
            animancer.Layers.SetMinCount(layerIndex + 1);
            layerState = new AnimationLayerPlaybackState(layerType, layerIndex);
            layerStates.Add(layerType, layerState);
            return layerState;
        }

        private float ResolveFadeDuration(
            AnimationDefinition definition,
            AnimationCommand command,
            AnimationLayerType layerType)
        {
            if (command.hasFadeDurationOverride)
                return Mathf.Max(0f, command.fadeDurationOverride);
            if (definition.fadeInDuration >= 0f)
                return definition.fadeInDuration;
            return profile.GetDefaultFadeDuration(layerType);
        }

        private static float ResolveSpeed(AnimationDefinition definition, AnimationCommand command)
        {
            if (command.hasSpeedOverride)
                return command.speedOverride;
            return definition.speed == 0f ? 1f : definition.speed;
        }

        private bool PlayFallbackLocomotion(LocomotionDefinition locomotion)
        {
            if (locomotion.fallbackAnimationId == 0)
                return false;

            var command = AnimationCommand.Create(locomotion.fallbackAnimationId);
            command.hasLayerOverride = true;
            command.layerOverride = locomotion.layer;
            command.ignoreInterruption = true;
            return PlayAnimation(command).Accepted;
        }

        private void UpdateLocomotion(float deltaTime)
        {
            if (profile == null || profile.Locomotion == null)
                return;

            var dampTime = profile.Locomotion.parameterDampTime;
            if (dampTime > 0f)
            {
                smoothedSpeed = Mathf.MoveTowards(smoothedSpeed, locomotionInput.speed, deltaTime / dampTime);
                smoothedLocalVelocity = Vector2.MoveTowards(
                    smoothedLocalVelocity,
                    locomotionInput.localVelocity,
                    deltaTime / dampTime);
            }
            else
            {
                smoothedSpeed = locomotionInput.speed;
                smoothedLocalVelocity = locomotionInput.localVelocity;
            }

            if (linearLocomotionState != null)
                linearLocomotionState.Parameter = smoothedSpeed;
            if (directionalLocomotionState != null)
                directionalLocomotionState.Parameter = smoothedLocalVelocity;
        }

        private void ClearCompletedLayers()
        {
            foreach (var layerState in layerStates.Values)
            {
                var definition = layerState.CurrentDefinition;
                if (definition == null ||
                    layerState.CurrentState == null ||
                    layerState.IsPlaying ||
                    !definition.clearLayerOnComplete)
                {
                    continue;
                }

                var layer = animancer.Layers[layerState.LayerIndex];
                layer.StartFade(0f, Mathf.Max(0f, definition.fadeOutDuration));
                eventScheduler.ClearLayer(layerState.Layer);
                layerState.Clear();
            }
        }

        private void ClearLocomotionState()
        {
            locomotionState = null;
            linearLocomotionState = null;
            directionalLocomotionState = null;
        }

        private void ApplyRootMotionPolicy(AnimationRootMotionPolicy policy)
        {
            if (rootMotionDriver != null)
                rootMotionDriver.Policy = policy;
        }

        private AnimationLayerPlaybackState ResolvePrimaryEventLayer()
        {
            AnimationLayerPlaybackState result = null;
            foreach (var layerState in layerStates.Values)
            {
                if (!layerState.IsPlaying)
                    continue;

                if (result == null || layerState.CurrentPriority > result.CurrentPriority)
                    result = layerState;
            }

            return result;
        }

        private void DispatchAnimationEvent(AnimationEventContext context)
        {
            AnimationEventTriggered?.Invoke(context);
            onAnimationEvent.Invoke(context);
        }
    }

    [Serializable]
    public sealed class AnimationEventUnityEvent : UnityEvent<AnimationEventContext>
    {
    }
}
