using UnityEngine;

namespace GameAnimation.Core
{
    public readonly struct RootMotionDelta
    {
        public Vector3 DeltaPosition { get; }
        public Quaternion DeltaRotation { get; }

        public RootMotionDelta(Vector3 deltaPosition, Quaternion deltaRotation)
        {
            DeltaPosition = deltaPosition;
            DeltaRotation = deltaRotation;
        }
    }
}
