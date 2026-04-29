using Combat.Runtime.Actors;
using Combat.Runtime.Attributes;
using Combat.Runtime.Core;
using Combat.Runtime.Effects;
using cfg;
using cfg.Config;

namespace Combat.Runtime.Data
{
    public static class LubanCombatDefinitionFactory
    {
        public static CombatActorDefinition CreateActorDefinition(
            ActorCfg actorConfiguration,
            string actorIdentifier,
            string teamIdentifier,
            CombatVector3 initialPosition,
            string displayName = null)
        {
            if (actorConfiguration == null) return null;

            return new CombatActorDefinition
            {
                ActorIdentifier = actorIdentifier,
                DisplayName = displayName ?? actorConfiguration.Id.ToString(),
                TeamIdentifier = teamIdentifier,
                InitialPosition = initialPosition,
                MaxHealth = actorConfiguration.MaxHp,
                MaxStamina = actorConfiguration.Stamina,
                StaminaRecovery = actorConfiguration.StaminaRegen,
                AttackPower = actorConfiguration.AttackPower,
                Defense = actorConfiguration.Defense,
                CriticalChance = actorConfiguration.CritRate,
                CriticalMultiplier = actorConfiguration.CritMultiplier,
                MoveSpeed = actorConfiguration.MoveSpeed,
                AbilityCooldownMultiplier = actorConfiguration.SkillCooldownMult <= 0f
                    ? 1f
                    : actorConfiguration.SkillCooldownMult,
                DamageDealtMultiplier = 1f,
                DamageTakenMultiplier = 1f,
            };
        }

        public static EffectDefinition CreateEffectDefinition(BuffCfg buffConfiguration, TbBuffEffect effectTable)
        {
            if (buffConfiguration == null) return null;

            var definition = new EffectDefinition
            {
                EffectIdentifier = buffConfiguration.BuffId,
                DisplayName = buffConfiguration.BuffId,
                Duration = buffConfiguration.DefaultDuration,
                MaxStacks = buffConfiguration.MaxStacks <= 0 ? 1 : buffConfiguration.MaxStacks,
                StackingPolicy = ResolveStackingPolicy(buffConfiguration.MaxStacks),
            };

            foreach (var effectIdentifier in buffConfiguration.EffectIds)
            {
                var effectConfiguration = effectTable?.GetOrDefault(effectIdentifier);
                if (effectConfiguration == null) continue;
                AppendBuffEffect(definition, effectConfiguration);
            }

            return definition;
        }

        public static void RegisterEffects(CombatDataRegistry registry, Tables tables)
        {
            if (registry == null || tables == null) return;

            foreach (var buffConfiguration in tables.TbBuff.DataList)
                registry.RegisterEffect(CreateEffectDefinition(buffConfiguration, tables.TbBuffEffect));
        }

        public static string ResolveAttributeIdentifier(StatType statType)
        {
            return statType switch
            {
                StatType.MaxHp => CombatAttributes.MaxHealth,
                StatType.Stamina => CombatAttributes.MaxStamina,
                StatType.StaminaRegen => CombatAttributes.StaminaRecovery,
                StatType.AttackPower => CombatAttributes.AttackPower,
                StatType.Defense => CombatAttributes.Defense,
                StatType.CritRate => CombatAttributes.CriticalChance,
                StatType.CritMultiplier => CombatAttributes.CriticalMultiplier,
                StatType.MoveSpeed => CombatAttributes.MoveSpeed,
                StatType.SkillCooldownMult => CombatAttributes.AbilityCooldownMultiplier,
                _ => null,
            };
        }

        public static AttributeModifierOperation ResolveModifierOperation(ModOp operation)
        {
            return operation == ModOp.Mul
                ? AttributeModifierOperation.Multiply
                : AttributeModifierOperation.Add;
        }

        private static EffectStackingPolicy ResolveStackingPolicy(int maxStacks)
            => maxStacks > 1 ? EffectStackingPolicy.AddStack : EffectStackingPolicy.RefreshDuration;

        private static void AppendBuffEffect(EffectDefinition definition, BuffEffectCfg effectConfiguration)
        {
            switch (effectConfiguration.EffectType)
            {
                case BuffEffectType.ModifyStat:
                    AddAttributeModifier(definition, effectConfiguration);
                    break;
                case BuffEffectType.DamageOverTime:
                    AddPeriodicExecution(definition, effectConfiguration, EffectExecutionType.Damage);
                    break;
                case BuffEffectType.HealOverTime:
                    AddPeriodicExecution(definition, effectConfiguration, EffectExecutionType.Heal);
                    break;
                case BuffEffectType.DamageAmplify:
                    definition.AttributeModifiers.Add(new AttributeModifierDefinition
                    {
                        AttributeIdentifier = CombatAttributes.DamageTakenMultiplier,
                        Operation = AttributeModifierOperation.Multiply,
                        Value = effectConfiguration.Value,
                    });
                    break;
                case BuffEffectType.Invincible:
                    definition.GrantedTags.Add(CombatTags.StateInvincible);
                    break;
                case BuffEffectType.Stun:
                    definition.GrantedTags.Add(CombatTags.StateStunned);
                    break;
            }
        }

        private static void AddAttributeModifier(EffectDefinition definition, BuffEffectCfg effectConfiguration)
        {
            var attributeIdentifier = ResolveAttributeIdentifier(effectConfiguration.TargetStat);
            if (string.IsNullOrEmpty(attributeIdentifier)) return;

            definition.AttributeModifiers.Add(new AttributeModifierDefinition
            {
                AttributeIdentifier = attributeIdentifier,
                Operation = ResolveModifierOperation(effectConfiguration.ModOp),
                Value = effectConfiguration.Value,
            });
        }

        private static void AddPeriodicExecution(
            EffectDefinition definition,
            BuffEffectCfg effectConfiguration,
            EffectExecutionType executionType)
        {
            if (effectConfiguration.TickInterval > 0f)
                definition.Period = effectConfiguration.TickInterval;

            definition.ExecutionsOnPeriod.Add(new EffectExecutionDefinition
            {
                ExecutionType = executionType,
                Value = effectConfiguration.Value,
            });
        }
    }
}
