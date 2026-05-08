using MessagePack;

namespace Battle.CombatInfo.Action
{
    [MessagePackObject(true)]
    public sealed class MotionData : InstructData
    {
        public override InstructType id => InstructType.Motion;

        public int motion;
    }

    [MessagePackObject(true)]
    public sealed class CreateBulletData : InstructData
    {
        public override InstructType id => InstructType.CreateBullet;

        public int bullet;
    }

    [MessagePackObject(true)]
    public sealed class AudioData : InstructData
    {
        public override InstructType id => InstructType.Audio;

        public int audio;
    }

    [MessagePackObject(true)]
    public sealed class CameraData : InstructData
    {
        public override InstructType id => InstructType.Camera;

        public int camera;
    }

    [MessagePackObject(true)]
    public sealed class VfxData : InstructData
    {
        public override InstructType id => InstructType.Vfx;

        public int vfx;
    }
}
