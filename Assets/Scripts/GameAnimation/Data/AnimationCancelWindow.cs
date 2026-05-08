using System;
using UnityEngine;

namespace GameAnimation.Data
{
    [Serializable]
    public sealed class AnimationCancelWindow
    {
        public int cancelGroupId;
        [Range(0f, 1f)] public float startNormalizedTime;
        [Range(0f, 1f)] public float endNormalizedTime = 1f;

        public bool Contains(float normalizedTime)
        {
            return normalizedTime >= startNormalizedTime && normalizedTime <= endNormalizedTime;
        }
    }
}
