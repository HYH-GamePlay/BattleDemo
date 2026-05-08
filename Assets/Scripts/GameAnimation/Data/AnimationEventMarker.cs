using System;
using GameAnimation.Core;
using UnityEngine;

namespace GameAnimation.Data
{
    [Serializable]
    public sealed class AnimationEventMarker
    {
        [Range(0f, 1f)] public float normalizedTime;
        public int eventId;
        public int payloadId;
        public AnimationEventType eventType = AnimationEventType.Custom;
    }
}
