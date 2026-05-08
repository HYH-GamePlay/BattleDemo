using System.Collections.Generic;
using GameAnimation.Core;
using UnityEngine;

namespace GameAnimation.Data
{
    [CreateAssetMenu(menuName = "Game Animation/Character Animation Profile", fileName = "CharacterAnimationProfile")]
    public sealed class CharacterAnimationProfile : ScriptableObject
    {
        [SerializeField] private List<AnimationLayerDefinition> layers = new();
        [SerializeField] private LocomotionDefinition locomotion = new();
        [SerializeField] private List<AnimationDefinition> animations = new();

        private readonly Dictionary<AnimationLayerType, AnimationLayerDefinition> layerByType = new();
        private readonly Dictionary<int, AnimationDefinition> animationById = new();
        private bool lookupDirty = true;

        public IReadOnlyList<AnimationLayerDefinition> Layers => layers;
        public LocomotionDefinition Locomotion => locomotion;
        public IReadOnlyList<AnimationDefinition> Animations => animations;

        private void OnEnable()
        {
            RebuildLookup();
        }

        private void OnValidate()
        {
            lookupDirty = true;
        }

        public bool TryGetAnimationDefinition(int animationId, out AnimationDefinition definition)
        {
            EnsureLookup();
            return animationById.TryGetValue(animationId, out definition);
        }

        public bool TryGetLayerDefinition(AnimationLayerType layer, out AnimationLayerDefinition definition)
        {
            EnsureLookup();
            return layerByType.TryGetValue(layer, out definition);
        }

        public int GetLayerIndex(AnimationLayerType layer)
        {
            return TryGetLayerDefinition(layer, out var definition)
                ? definition.layerIndex
                : GetFallbackLayerIndex(layer);
        }

        public float GetDefaultFadeDuration(AnimationLayerType layer)
        {
            return TryGetLayerDefinition(layer, out var definition)
                ? definition.defaultFadeDuration
                : 0.15f;
        }

        private void EnsureLookup()
        {
            if (lookupDirty)
                RebuildLookup();
        }

        private void RebuildLookup()
        {
            layerByType.Clear();
            animationById.Clear();

            foreach (var layer in layers)
            {
                if (layer == null) continue;
                if (!layerByType.ContainsKey(layer.layer))
                    layerByType.Add(layer.layer, layer);
            }

            foreach (var animation in animations)
            {
                if (animation == null || animation.animationId == 0) continue;
                if (!animationById.ContainsKey(animation.animationId))
                    animationById.Add(animation.animationId, animation);
            }

            lookupDirty = false;
        }

        private static int GetFallbackLayerIndex(AnimationLayerType layer)
        {
            switch (layer)
            {
                case AnimationLayerType.Base:
                case AnimationLayerType.Locomotion:
                    return 0;
                case AnimationLayerType.Action:
                    return 1;
                case AnimationLayerType.UpperBody:
                    return 2;
                case AnimationLayerType.Reaction:
                    return 3;
                case AnimationLayerType.Additive:
                    return 4;
                case AnimationLayerType.Overlay:
                    return 5;
                default:
                    return 0;
            }
        }
    }
}
