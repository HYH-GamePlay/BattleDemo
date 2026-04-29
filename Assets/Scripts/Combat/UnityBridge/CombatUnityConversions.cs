using Combat.Runtime.Core;
using UnityEngine;

namespace Combat.UnityBridge
{
    public static class CombatUnityConversions
    {
        public static CombatVector3 ToCombatVector3(this Vector3 value)
            => new(value.x, value.y, value.z);

        public static Vector3 ToUnityVector3(this CombatVector3 value)
            => new(value.X, value.Y, value.Z);
    }
}
