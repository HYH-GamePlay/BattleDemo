using UnityEngine;

namespace Combat.View
{
    /// <summary>
    /// 实体视图 — 只持有 entityId，只订阅 EventBus，不持有逻辑层引用。
    /// Transform 同步由 TransformSyncSystem 统一批处理。
    /// </summary>
    public class EntityView : MonoBehaviour
    {
        [SerializeField] private AnimationView _animationView;
        [SerializeField] private VFXView _vfxView;
        [SerializeField] private AudioView _audioView;
        [SerializeField] private HealthBarView _healthBar;

        private string _entityId;
        private Core.EventBus _bus;

        public string EntityId => _entityId;

        public void Initialize(string entityId, Core.EventBus bus)
        {
            _entityId = entityId;
            _bus = bus;
            bus.Subscribe<Core.DamageTakenEvent>(OnDamageTaken);
            bus.Subscribe<Core.HealedEvent>(OnHealed);
            bus.Subscribe<Core.StateChangedEvent>(OnStateChanged);
            bus.Subscribe<Core.EntityDeathEvent>(OnDeath);
            bus.Subscribe<Core.PerfectBlockEvent>(OnPerfectBlock);
            bus.Subscribe<Core.CounterEvent>(OnCounter);
            bus.Subscribe<Core.VitalChangedEvent>(OnVitalChanged);
        }

        private void OnDamageTaken(Core.DamageTakenEvent e)
        {
            if (e.EntityId != _entityId) return;
            _animationView?.PlayAction(_animationView.DamageClip);
            _vfxView?.PlayHitEffect();
            _audioView?.PlayHitSound();
        }

        private void OnHealed(Core.HealedEvent e)
        {
            if (e.EntityId != _entityId) return;
            _vfxView?.PlayHealEffect();
        }

        private void OnStateChanged(Core.StateChangedEvent e)
        {
            if (e.EntityId != _entityId) return;
            switch (e.NewState)
            {
                case nameof(FSM.States.AttackState):  _animationView?.PlayAction(_animationView.AttackClip);      break;
                case nameof(FSM.States.DefendState):  _animationView?.PlayAction(_animationView.BlockClip);       break;
                case nameof(FSM.States.DodgeState):   _animationView?.PlayAction(_animationView.DodgeClip);       break;
                case nameof(FSM.States.IdleState):    _animationView?.SetMoveSpeed(0f);                           break;
                case nameof(FSM.States.DeadState):    _animationView?.PlayAction(_animationView.DeathClip);       break;
            }
        }

        private void OnDeath(Core.EntityDeathEvent e)
        {
            if (e.EntityId != _entityId) return;
            _vfxView?.PlayDeathEffect();
            _audioView?.PlayDeathSound();
        }

        private void OnPerfectBlock(Core.PerfectBlockEvent e)
        {
            if (e.Defender?.EntityId != _entityId) return;
            _animationView?.PlayAction(_animationView.PerfectBlockClip);
            _vfxView?.PlayPerfectBlockEffect();
            _audioView?.PlayPerfectBlockSound();
        }

        private void OnCounter(Core.CounterEvent e)
        {
            if (e.AttackerId != _entityId) return;
            _animationView?.PlayAction(_animationView.CounterClip);
            _vfxView?.PlayCounterEffect();
            _audioView?.PlayCounterSound();
        }

        private void OnVitalChanged(Core.VitalChangedEvent e)
        {
            if (e.EntityId != _entityId) return;
            _healthBar?.SetHealth(e.CurrentHp, e.MaxHp);
            _healthBar?.SetStamina(e.CurrentStamina, e.MaxStamina);
        }

        private void OnDestroy()
        {
            if (_bus == null) return;
            _bus.Unsubscribe<Core.DamageTakenEvent>(OnDamageTaken);
            _bus.Unsubscribe<Core.HealedEvent>(OnHealed);
            _bus.Unsubscribe<Core.StateChangedEvent>(OnStateChanged);
            _bus.Unsubscribe<Core.EntityDeathEvent>(OnDeath);
            _bus.Unsubscribe<Core.PerfectBlockEvent>(OnPerfectBlock);
            _bus.Unsubscribe<Core.CounterEvent>(OnCounter);
            _bus.Unsubscribe<Core.VitalChangedEvent>(OnVitalChanged);
        }
    }
}
