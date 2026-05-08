using GameAnimation.Core;
using UnityEngine;

namespace GameAnimation.UnityBridge
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class RootMotionDriver : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Transform motionRoot;
        [SerializeField] private AnimationRootMotionPolicy policy = AnimationRootMotionPolicy.AccumulateForConsumer;

        private Vector3 pendingDeltaPosition;
        private Quaternion pendingDeltaRotation = Quaternion.identity;
        private bool hasPendingRotation;

        public AnimationRootMotionPolicy Policy
        {
            get => policy;
            set => policy = value;
        }

        public Vector3 PendingDeltaPosition => pendingDeltaPosition;
        public Quaternion PendingDeltaRotation => hasPendingRotation ? pendingDeltaRotation : Quaternion.identity;

        private void Reset()
        {
            animator = GetComponent<Animator>();
            motionRoot = transform;
        }

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            if (motionRoot == null)
                motionRoot = transform;
        }

        private void OnAnimatorMove()
        {
            if (animator == null) return;

            var deltaPosition = animator.deltaPosition;
            var deltaRotation = animator.deltaRotation;

            switch (policy)
            {
                case AnimationRootMotionPolicy.Ignore:
                    break;
                case AnimationRootMotionPolicy.AccumulateForConsumer:
                    Accumulate(deltaPosition, deltaRotation);
                    break;
                case AnimationRootMotionPolicy.ApplyToTransform:
                    Apply(deltaPosition, deltaRotation);
                    break;
            }
        }

        public RootMotionDelta ConsumeRootMotion()
        {
            var result = new RootMotionDelta(
                pendingDeltaPosition,
                hasPendingRotation ? pendingDeltaRotation : Quaternion.identity);

            pendingDeltaPosition = Vector3.zero;
            pendingDeltaRotation = Quaternion.identity;
            hasPendingRotation = false;
            return result;
        }

        public void Clear()
        {
            pendingDeltaPosition = Vector3.zero;
            pendingDeltaRotation = Quaternion.identity;
            hasPendingRotation = false;
        }

        private void Accumulate(Vector3 deltaPosition, Quaternion deltaRotation)
        {
            pendingDeltaPosition += deltaPosition;
            pendingDeltaRotation = hasPendingRotation
                ? pendingDeltaRotation * deltaRotation
                : deltaRotation;
            hasPendingRotation = true;
        }

        private void Apply(Vector3 deltaPosition, Quaternion deltaRotation)
        {
            if (motionRoot == null) return;

            motionRoot.position += deltaPosition;
            motionRoot.rotation *= deltaRotation;
        }
    }
}
