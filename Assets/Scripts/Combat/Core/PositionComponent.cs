using Unity.Mathematics;

namespace Combat.Core
{
    public class PositionComponent
    {
        public float2 Position;
        public float Rotation; // Y轴旋转角度（度）

        public float X => Position.x;
        public float Z => Position.y;

        public void Set(float x, float z) => Position = new float2(x, z);
    }
}
