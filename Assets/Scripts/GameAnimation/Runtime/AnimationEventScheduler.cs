using System;
using System.Collections.Generic;
using Animancer;
using GameAnimation.Core;
using GameAnimation.Data;
using UnityEngine;

namespace GameAnimation.Runtime
{
    internal sealed class AnimationEventScheduler
    {
        private readonly List<ScheduledAnimationEvent> scheduledEvents = new();

        public void Schedule(
            int actorId,
            AnimationDefinition definition,
            AnimancerState state,
            AnimationLayerType layer)
        {
            if (definition == null || state == null) return;

            ClearLayer(layer);

            var startNormalizedTime = Mathf.Clamp01(state.NormalizedTime);
            foreach (var marker in definition.eventMarkers)
            {
                if (marker == null) continue;

                scheduledEvents.Add(new ScheduledAnimationEvent(
                    actorId,
                    definition.animationId,
                    marker,
                    state,
                    layer,
                    marker.normalizedTime < startNormalizedTime));
            }
        }

        public void ClearLayer(AnimationLayerType layer)
        {
            for (var i = scheduledEvents.Count - 1; i >= 0; i--)
            {
                if (scheduledEvents[i].Layer == layer)
                    scheduledEvents.RemoveAt(i);
            }
        }

        public void Tick(Action<AnimationEventContext> dispatch)
        {
            if (dispatch == null) return;

            for (var i = scheduledEvents.Count - 1; i >= 0; i--)
            {
                var scheduledEvent = scheduledEvents[i];
                if (scheduledEvent.State == null)
                {
                    scheduledEvents.RemoveAt(i);
                    continue;
                }

                if (scheduledEvent.Invoked)
                    continue;

                var normalizedTime = scheduledEvent.State.NormalizedTime;
                if (scheduledEvent.State.IsLooping)
                    normalizedTime -= Mathf.Floor(normalizedTime);

                if (normalizedTime < scheduledEvent.Marker.normalizedTime)
                    continue;

                scheduledEvent.Invoked = true;
                dispatch(new AnimationEventContext(
                    scheduledEvent.ActorId,
                    scheduledEvent.AnimationId,
                    scheduledEvent.Marker.eventId,
                    scheduledEvent.Marker.payloadId,
                    scheduledEvent.Marker.eventType,
                    AnimationEventOrigin.DataMarker,
                    scheduledEvent.Layer,
                    normalizedTime));
            }
        }

        private sealed class ScheduledAnimationEvent
        {
            public int ActorId { get; }
            public int AnimationId { get; }
            public AnimationEventMarker Marker { get; }
            public AnimancerState State { get; }
            public AnimationLayerType Layer { get; }
            public bool Invoked { get; set; }

            public ScheduledAnimationEvent(
                int actorId,
                int animationId,
                AnimationEventMarker marker,
                AnimancerState state,
                AnimationLayerType layer,
                bool invoked)
            {
                ActorId = actorId;
                AnimationId = animationId;
                Marker = marker;
                State = state;
                Layer = layer;
                Invoked = invoked;
            }
        }
    }
}
