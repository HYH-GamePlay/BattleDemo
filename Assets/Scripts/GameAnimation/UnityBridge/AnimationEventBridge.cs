using GameAnimation.Core;
using UnityEngine;

namespace GameAnimation.UnityBridge
{
    public sealed class AnimationEventBridge : MonoBehaviour
    {
        [SerializeField] private CharacterAnimationActor actor;

        private void Awake()
        {
            if (actor == null)
                actor = GetComponentInParent<CharacterAnimationActor>();
        }

        public void DispatchAnimationEvent(int eventId)
        {
            if (actor != null)
                actor.DispatchUnityAnimationEvent(eventId, 0, AnimationEventType.Custom);
        }

        public void DispatchAnimationEvent(AnimationEvent animationEvent)
        {
            if (actor == null || animationEvent == null) return;

            actor.DispatchUnityAnimationEvent(
                animationEvent.intParameter,
                0,
                AnimationEventType.Custom);
        }

        public void DispatchFootstepEvent(int eventId)
        {
            if (actor != null)
                actor.DispatchUnityAnimationEvent(eventId, 0, AnimationEventType.Footstep);
        }
    }
}
