using System;

namespace Combat.Runtime.Core
{
    [Serializable]
    public struct CombatVector3
    {
        public float X;
        public float Y;
        public float Z;

        public CombatVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static CombatVector3 Zero => new(0f, 0f, 0f);
        public static CombatVector3 Forward => new(0f, 0f, 1f);

        public float SqrMagnitude => X * X + Y * Y + Z * Z;

        public CombatVector3 Normalized
        {
            get
            {
                var magnitude = (float)Math.Sqrt(SqrMagnitude);
                return magnitude <= 0.0001f ? Forward : this / magnitude;
            }
        }

        public static float DistanceSquared(CombatVector3 a, CombatVector3 b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            var dz = a.Z - b.Z;
            return dx * dx + dy * dy + dz * dz;
        }

        public static CombatVector3 operator +(CombatVector3 a, CombatVector3 b)
            => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static CombatVector3 operator -(CombatVector3 a, CombatVector3 b)
            => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        public static CombatVector3 operator *(CombatVector3 value, float scale)
            => new(value.X * scale, value.Y * scale, value.Z * scale);

        public static CombatVector3 operator /(CombatVector3 value, float divisor)
            => new(value.X / divisor, value.Y / divisor, value.Z / divisor);
    }
}
