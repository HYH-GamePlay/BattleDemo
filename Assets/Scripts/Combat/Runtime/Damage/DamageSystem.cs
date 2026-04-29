using Combat.Runtime.Core;
using Combat.Runtime.Events;

namespace Combat.Runtime.Damage
{
    public sealed class DamageSystem
    {
        private readonly CombatWorld _world;

        public DamageSystem(CombatWorld world)
        {
            _world = world;
        }

        public DamageResult Resolve(DamageRequest request)
        {
            _world.TryGetActor(request.SourceActorIdentifier, out var source);
            if (!_world.TryGetActor(request.TargetActorIdentifier, out var target) || target.IsDead)
                return new DamageResult(request.SourceActorIdentifier, request.TargetActorIdentifier, 0f, false, request.DamageTypeTag);

            if (target.Tags.Has(CombatTags.StateInvincible))
                return new DamageResult(request.SourceActorIdentifier, request.TargetActorIdentifier, 0f, false, request.DamageTypeTag);

            var attackPower = source?.Attributes.GetValue(CombatAttributes.AttackPower) ?? 1f;
            var damageDealtMultiplier = source?.Attributes.GetValue(CombatAttributes.DamageDealtMultiplier) ?? 1f;
            var damageTakenMultiplier = target.Attributes.GetValue(CombatAttributes.DamageTakenMultiplier);
            var defenseScale = ResolveDefenseScale(target);
            var amount = request.BaseDamage * attackPower * damageDealtMultiplier * damageTakenMultiplier * (1f - defenseScale);
            var isCritical = false;

            if (request.CanCritical && source != null && _world.NextRandom01() < source.Attributes.GetValue(CombatAttributes.CriticalChance))
            {
                amount *= source.Attributes.GetValue(CombatAttributes.CriticalMultiplier);
                isCritical = true;
            }

            amount = System.Math.Max(0f, amount);
            _world.ChangeCurrentAttribute(target, CombatAttributes.Health, -amount, 0f, target.MaxHealth);
            if (target.Health <= 0f)
            {
                _world.AddTag(target, CombatTags.StateDead);
                _world.Events.Publish(new ActorDeathEvent
                {
                    ActorIdentifier = target.ActorIdentifier,
                    KillerActorIdentifier = source?.ActorIdentifier,
                });
            }

            var result = new DamageResult(request.SourceActorIdentifier, request.TargetActorIdentifier, amount, isCritical, request.DamageTypeTag);
            _world.Events.Publish(new DamageResolvedEvent
            {
                SourceActorIdentifier = result.SourceActorIdentifier,
                TargetActorIdentifier = result.TargetActorIdentifier,
                Amount = result.FinalDamage,
                IsCritical = result.IsCritical,
                DamageTypeTag = result.DamageTypeTag,
            });
            return result;
        }

        private float ResolveDefenseScale(Actors.CombatActor target)
        {
            var range = System.Math.Max(0.001f, _world.Options.DefenseScaleRange);
            var ratio = target.Attributes.GetValue(CombatAttributes.Defense) / range;
            return CombatMath.Clamp(CombatMath.Lerp(_world.Options.DefenseScaleMinimum, _world.Options.DefenseScaleMaximum, ratio), 0f, 1f);
        }
    }
}
