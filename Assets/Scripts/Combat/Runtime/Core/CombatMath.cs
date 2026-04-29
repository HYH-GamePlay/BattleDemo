using System;

namespace Combat.Runtime.Core
{
    public static class CombatMath
    {
        public static float Clamp(float value, float min, float max)
            => MathF.Min(max, MathF.Max(min, value));

        public static float Clamp01(float value) => Clamp(value, 0f, 1f);

        public static float Lerp(float from, float to, float ratio)
            => from + (to - from) * Clamp01(ratio);
    }
}
