namespace Combat.Core
{
    public struct HitResult
    {
        public float FinalDamage;
        public bool IsCrit;
        public bool WasPerfectBlock;
        public bool WasBlocked;
    }

    public interface IHitProcessor
    {
        void Process(ref HitContext ctx, ref HitResult result);
    }
}
