using System;
using GameAnimation.Core;
using UnityEngine;

namespace GameAnimation.Data
{
    [Serializable]
    public sealed class AnimationLayerDefinition
    {
        public AnimationLayerType layer = AnimationLayerType.Base;
        public int layerIndex;
        public AvatarMask avatarMask;
        public bool additive;
        [Range(0f, 1f)] public float defaultWeight = 1f;
        public float defaultFadeDuration = 0.15f;
    }
}
