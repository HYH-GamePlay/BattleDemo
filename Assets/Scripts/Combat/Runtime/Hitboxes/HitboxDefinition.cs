using Combat.Runtime.Core;

namespace Combat.Runtime.Hitboxes
{
    [System.Serializable]
    public sealed class HitboxDefinition
    {
        public string HitboxIdentifier;
        public float Radius = 1f;
        public CombatVector3 Offset;
        public bool HitEnemies = true;
        public bool HitAllies;
        public bool HitSelf;
        public float Damage;
        public string DamageTypeTag;
        public string EffectIdentifier;
        public string CueIdentifier;
    }
}
