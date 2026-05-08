using System;
using System.Collections.Generic;
using GameAnimation.Core;
using UnityEngine;

namespace GameAnimation.Data
{
    [Serializable]
    public sealed class AnimationDefinition
    {
        public int animationId;
        public string displayName;
        public AnimationClip clip;
        public AnimationLayerType layer = AnimationLayerType.Action;
        public int priority;
        public bool canBeInterrupted = true;
        public bool restartOnPlay = true;
        public bool clearLayerOnComplete = true;
        public float fadeInDuration = 0.12f;
        public float fadeOutDuration = 0.12f;
        public float speed = 1f;
        public AnimationRootMotionPolicy rootMotionPolicy = AnimationRootMotionPolicy.Ignore;
        public List<AnimationEventMarker> eventMarkers = new();
        public List<AnimationCancelWindow> cancelWindows = new();
    }
}
