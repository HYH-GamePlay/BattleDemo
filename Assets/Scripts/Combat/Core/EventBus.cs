using System;
using System.Collections.Generic;

namespace Combat.Core
{
    /// <summary>
    /// 战斗事件基类
    /// </summary>
    public interface ICombatEvent { }

    // 伤害/生命事件
    public struct DamageTakenEvent : ICombatEvent
    {
        public string EntityId;
        public float Amount;
        public bool IsCrit;
        public string AttackerId;
    }

    public struct HealedEvent : ICombatEvent
    {
        public string EntityId;
        public float Amount;
    }

    public struct EntityDeathEvent : ICombatEvent
    {
        public string EntityId;
        public string KillerId;
    }

    public struct VitalChangedEvent : ICombatEvent
    {
        public string EntityId;
        public float CurrentHp;
        public float MaxHp;
        public float CurrentStamina;
        public float MaxStamina;
    }

    // 逻辑层战斗事件
    public struct DamageResult
    {
        public float FinalDamage;
        public bool IsCrit;
        public CombatEntity Attacker;
        public CombatEntity Defender;

        public DamageResult(float damage, bool isCrit, CombatEntity attacker, CombatEntity defender)
        {
            FinalDamage = damage;
            IsCrit = isCrit;
            Attacker = attacker;
            Defender = defender;
        }
    }

    public struct HitEvent : ICombatEvent
    {
        public DamageResult Result;
        public CombatEntity Attacker;
        public CombatEntity Defender;
    }

    public struct KillEvent : ICombatEvent
    {
        public CombatEntity Killer;
        public CombatEntity Victim;
    }

    // 战斗机制事件
    public struct PerfectBlockEvent : ICombatEvent
    {
        public CombatEntity Defender;
        public CombatEntity Attacker;
    }

    public struct CounterEvent : ICombatEvent
    {
        public string AttackerId;
        public string TargetId;
        public float Damage;
    }

    public struct DodgeEvent : ICombatEvent
    {
        public string EntityId;
    }

    public struct StateChangedEvent : ICombatEvent
    {
        public string EntityId;
        public string OldState;
        public string NewState;
    }

    // 技能表现事件（View层消费）
    public struct SkillVFXEvent : ICombatEvent
    {
        public string VfxKey;
        public float PosX;
        public float PosZ;
        public string TargetEntityId;
    }

    public struct SkillAudioEvent : ICombatEvent
    {
        public string AudioKey;
        public float Volume;
        public string SourceEntityId;
    }

    /// <summary>
    /// 事件总线 - 纯C#实现，逻辑层与表现层通信
    /// </summary>
    public class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Publish<T>(T evt) where T : struct, ICombatEvent
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list)) return;

            foreach (var handler in list.ToArray())
            {
                ((Action<T>)handler)?.Invoke(evt);
            }
        }

        public void Subscribe<T>(Action<T> handler) where T : struct, ICombatEvent
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers[type] = list;
            }
            list.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct, ICombatEvent
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var list))
            {
                list.Remove(handler);
            }
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}
