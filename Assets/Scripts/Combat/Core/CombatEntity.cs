namespace Combat.Core
{
    public enum EntityType { Player, Enemy }

    /// <summary>
    /// 战斗实体 — 纯数据容器，无 C# event，无 Unity 依赖。
    /// 所有状态变化通过 EventBus 发布。
    /// </summary>
    public class CombatEntity
    {
        public string EntityId { get; }
        public EntityType Type { get; }

        public StatSheet Stats { get; } = new();
        public VitalComponent Vitals { get; }
        public PositionComponent Position { get; } = new();
        public FSM.EntityFsm Fsm { get; private set; }

        public bool IsDead => Vitals.IsDead;
        public string State => Fsm?.CurrentStateName ?? "";
        public float Hp => Vitals.Hp;
        public float Stamina => Vitals.Stamina;
        public float PositionX { get => Position.X; set => Position.Set(value, Position.Z); }
        public float PositionZ { get => Position.Z; set => Position.Set(Position.X, value); }

        public CombatEntity(string entityId, EntityType type)
        {
            EntityId = entityId;
            Type = type;
            Vitals = new VitalComponent(entityId);
        }

        public void Initialize(cfg.Config.ActorCfg actorCfg, EventBus bus)
        {
            Stats.InitFromData(actorCfg);
            Vitals.Initialize(
                Stats.Get(global::cfg.StatType.MaxHp),
                Stats.Get(global::cfg.StatType.Stamina));
            Fsm = new FSM.EntityFsm(this, bus);
        }

        public void Tick(float delta, EventBus bus)
        {
            if (IsDead) return;
            Vitals.RegenerateStamina(delta, Stats.Get(global::cfg.StatType.StaminaRegen), bus);
            Fsm?.Tick(delta);
        }

        public void Reset(EventBus bus)
        {
            Stats.ClearRunModifiers();
            Vitals.Reset(Stats.Get(global::cfg.StatType.MaxHp), Stats.Get(global::cfg.StatType.Stamina));
            Fsm?.TransitionTo<FSM.States.IdleState>();
        }
    }
}
