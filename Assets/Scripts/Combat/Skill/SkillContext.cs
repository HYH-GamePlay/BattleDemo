namespace Combat.Skill
{
    /// <summary>
    /// 技能上下文 — 节点间共享数据，持有 EventBus 引用供节点发布事件。
    /// </summary>
    public class SkillContext
    {
        public Core.CombatEntity Caster;
        public Core.CombatEntity Target;
        public UnityEngine.Vector3 CastPosition;
        public UnityEngine.Vector3 AimDirection;
        public float DamageMultiplier = 1f;
        public bool IsPerfectBlock;
        public Core.EventBus Bus;
        public System.Collections.Generic.Dictionary<string, object> Blackboard = new();
    }
}
