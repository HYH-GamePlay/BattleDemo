using System;
using UnityEngine;

namespace GameAnimation.Core
{
    [Serializable]
    public struct AnimationLocomotionInput
    {
        public Vector3 worldVelocity;
        public Vector2 localVelocity;
        public float speed;
        public bool isGrounded;
        public bool isLockedOn;

        public static AnimationLocomotionInput FromWorldVelocity(
            Vector3 worldVelocity,
            Transform actorTransform,
            bool isGrounded,
            bool isLockedOn)
        {
            var localVelocity = actorTransform != null
                ? actorTransform.InverseTransformDirection(worldVelocity)
                : worldVelocity;

            return new AnimationLocomotionInput
            {
                worldVelocity = worldVelocity,
                localVelocity = new Vector2(localVelocity.x, localVelocity.z),
                speed = worldVelocity.magnitude,
                isGrounded = isGrounded,
                isLockedOn = isLockedOn,
            };
        }
    }
}
