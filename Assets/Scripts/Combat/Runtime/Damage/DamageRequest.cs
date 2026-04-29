namespace Combat.Runtime.Damage
{
    public readonly struct DamageRequest
    {
        public readonly string SourceActorIdentifier;
        public readonly string TargetActorIdentifier;
        public readonly float BaseDamage;
        public readonly string DamageTypeTag;
        public readonly bool CanCritical;

        public DamageRequest(string sourceActorIdentifier, string targetActorIdentifier, float baseDamage, string damageTypeTag, bool canCritical = true)
        {
            SourceActorIdentifier = sourceActorIdentifier;
            TargetActorIdentifier = targetActorIdentifier;
            BaseDamage = baseDamage;
            DamageTypeTag = damageTypeTag;
            CanCritical = canCritical;
        }
    }

    public readonly struct DamageResult
    {
        public readonly string SourceActorIdentifier;
        public readonly string TargetActorIdentifier;
        public readonly float FinalDamage;
        public readonly bool IsCritical;
        public readonly string DamageTypeTag;

        public DamageResult(string sourceActorIdentifier, string targetActorIdentifier, float finalDamage, bool isCritical, string damageTypeTag)
        {
            SourceActorIdentifier = sourceActorIdentifier;
            TargetActorIdentifier = targetActorIdentifier;
            FinalDamage = finalDamage;
            IsCritical = isCritical;
            DamageTypeTag = damageTypeTag;
        }
    }
}
