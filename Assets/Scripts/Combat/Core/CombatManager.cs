using System.Collections.Generic;

namespace Combat.Core
{
    /// <summary>
    /// 战斗管理器 — 顶层协调器，通过构造函数注入依赖。
    /// </summary>
    public class CombatManager
    {
        public EventBus EventBus { get; }
        public Buff.BuffSystem BuffSystem { get; }
        public HitPipeline HitPipeline { get; }
        public Skill.SkillSystem SkillSystem { get; }
        public Rogue.RelicSystem RelicSystem { get; }

        private readonly cfg.Config.ConstantCfg _constants;
        private readonly cfg.Tables _tables;
        private readonly Dictionary<string, CombatEntity> _entities = new();
        private readonly List<CombatEntity> _players = new();
        private readonly List<CombatEntity> _enemies = new();

        public cfg.Tables Tables => _tables;

        public CombatManager(cfg.Tables tables)
        {
            _tables    = tables;
            _constants = tables.TbConstant.DataList[0];
            EventBus   = new EventBus();
            BuffSystem = new Buff.BuffSystem(EventBus, tables);
            HitPipeline = new HitPipeline(_constants, BuffSystem);
            SkillSystem = new Skill.SkillSystem(HitPipeline, BuffSystem);
            RelicSystem = new Rogue.RelicSystem(EventBus, SkillSystem);
        }

        public CombatEntity CreateEntity(string entityId, EntityType type, cfg.Config.ActorCfg stats)
        {
            var entity = new CombatEntity(entityId, type);
            entity.Initialize(stats, EventBus);
            BuffSystem.Register(entity);
            _entities[entityId] = entity;
            (type == EntityType.Player ? _players : _enemies).Add(entity);
            return entity;
        }

        public void RemoveEntity(string entityId)
        {
            if (!_entities.TryGetValue(entityId, out var e)) return;
            BuffSystem.Unregister(entityId);
            _entities.Remove(entityId);
            (e.Type == EntityType.Player ? _players : _enemies).Remove(e);
        }

        public CombatEntity GetEntity(string entityId)
            => _entities.TryGetValue(entityId, out var e) ? e : null;

        public IReadOnlyList<CombatEntity> GetAllPlayers() => _players;
        public IReadOnlyList<CombatEntity> GetAllEnemies() => _enemies;

        public void Tick(float delta)
        {
            foreach (var e in _entities.Values) e.Tick(delta, EventBus);
            BuffSystem.Tick(delta);
            SkillSystem.Tick(delta);
        }

        public void ExecuteAttack(CombatEntity attacker, CombatEntity target, float baseDamage)
            => HitPipeline.Process(new HitContext { Attacker = attacker, Defender = target, BaseDamage = baseDamage, Bus = EventBus });

        public bool TryStartDefend(CombatEntity entity)
            => entity.Fsm.TransitionTo<FSM.States.DefendState>();

        public bool TryEndDefend(CombatEntity entity)
            => entity.Fsm.TransitionTo<FSM.States.IdleState>();

        public bool TryDodge(CombatEntity entity)
        {
            if (!entity.Vitals.UseStamina(_constants.DodgeStaminaCost)) return false;
            entity.Fsm.SetData("dodgeDuration", _constants.DodgeDuration);
            return entity.Fsm.TransitionTo<FSM.States.DodgeState>();
        }

        public bool TryAttack(CombatEntity entity, float duration)
        {
            entity.Fsm.SetData("attackDuration", duration);
            return entity.Fsm.TransitionTo<FSM.States.AttackState>();
        }

        public void Clear()
        {
            _entities.Clear(); _players.Clear(); _enemies.Clear();
            EventBus.Clear(); SkillSystem.Clear();
        }
    }
}
