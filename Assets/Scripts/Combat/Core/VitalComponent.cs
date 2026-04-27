using System;

namespace Combat.Core
{
    public class VitalComponent
    {
        public float Hp { get; private set; }
        public float Stamina { get; private set; }
        public bool IsDead => Hp <= 0f;

        private readonly string _entityId;
        private float _maxHp;
        private float _maxStamina;

        public VitalComponent(string entityId)
        {
            _entityId = entityId;
        }

        public void Initialize(float maxHp, float maxStamina)
        {
            _maxHp = maxHp;
            _maxStamina = maxStamina;
            Hp = maxHp;
            Stamina = maxStamina;
        }

        public void TakeDamage(float amount, CombatEntity attacker, EventBus bus)
        {
            if (IsDead) return;
            Hp = Math.Max(0f, Hp - amount);
            bus.Publish(new DamageTakenEvent { EntityId = _entityId, Amount = amount, AttackerId = attacker?.EntityId });
            bus.Publish(new VitalChangedEvent { EntityId = _entityId, CurrentHp = Hp, MaxHp = _maxHp, CurrentStamina = Stamina, MaxStamina = _maxStamina });
            if (IsDead)
                bus.Publish(new EntityDeathEvent { EntityId = _entityId, KillerId = attacker?.EntityId });
        }

        public void Heal(float amount, EventBus bus)
        {
            if (IsDead) return;
            Hp = Math.Min(_maxHp, Hp + amount);
            bus.Publish(new HealedEvent { EntityId = _entityId, Amount = amount });
            bus.Publish(new VitalChangedEvent { EntityId = _entityId, CurrentHp = Hp, MaxHp = _maxHp, CurrentStamina = Stamina, MaxStamina = _maxStamina });
        }

        public bool UseStamina(float amount)
        {
            if (Stamina < amount) return false;
            Stamina -= amount;
            return true;
        }

        public void RegenerateStamina(float delta, float regenRate, EventBus bus)
        {
            if (IsDead) return;
            var prev = Stamina;
            Stamina = Math.Min(_maxStamina, Stamina + regenRate * delta);
            if (Math.Abs(Stamina - prev) > 0.001f)
                bus.Publish(new VitalChangedEvent { EntityId = _entityId, CurrentHp = Hp, MaxHp = _maxHp, CurrentStamina = Stamina, MaxStamina = _maxStamina });
        }

        public void UpdateMaxValues(float maxHp, float maxStamina)
        {
            _maxHp = maxHp;
            _maxStamina = maxStamina;
        }

        public void Reset(float maxHp, float maxStamina)
        {
            Initialize(maxHp, maxStamina);
        }
    }
}
