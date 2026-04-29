using System.Collections.Generic;
using Combat.Runtime.Attributes;
using Combat.Runtime.Actors;
using Combat.Runtime.Core;
using Combat.Runtime.Cues;
using Combat.Runtime.Damage;
using Combat.Runtime.Events;

namespace Combat.Runtime.Effects
{
    public sealed class EffectSystem
    {
        private readonly CombatWorld _world;
        private readonly Dictionary<string, List<ActiveEffect>> _effectsByTarget = new();
        private readonly List<string> _targetTickBuffer = new();

        public EffectSystem(CombatWorld world)
        {
            _world = world;
        }

        public bool Apply(EffectApplicationRequest request)
        {
            if (!_world.DataRegistry.TryGetEffect(request.EffectIdentifier, out var definition)) return false;
            if (!_world.TryGetActor(request.TargetActorIdentifier, out var target)) return false;

            _world.TryGetActor(request.SourceActorIdentifier, out var source);
            ExecuteAll(definition.ExecutionsOnApply, source?.ActorIdentifier, target.ActorIdentifier);

            if (definition.IsInstant)
                return true;

            var list = GetList(target.ActorIdentifier);
            var existing = definition.StackingPolicy == EffectStackingPolicy.Independent
                ? null
                : list.Find(effect => effect.Definition.EffectIdentifier == definition.EffectIdentifier);

            if (existing != null)
            {
                existing.RemainingTime = definition.Duration;
                if (definition.StackingPolicy == EffectStackingPolicy.AddStack)
                    existing.StackCount = System.Math.Min(definition.MaxStacks, existing.StackCount + 1);
                RefreshModifiers(existing, target);
                PublishApplied(request, existing.StackCount);
                return true;
            }

            var activeEffect = new ActiveEffect(definition, source?.ActorIdentifier, target.ActorIdentifier);
            list.Add(activeEffect);
            AddModifiers(activeEffect, target);
            AddTags(activeEffect, target);
            _world.ClampVitalAttributes(target);
            PublishApplied(request, activeEffect.StackCount);
            return true;
        }

        public void Tick(float delta)
        {
            _targetTickBuffer.Clear();
            _targetTickBuffer.AddRange(_effectsByTarget.Keys);

            foreach (var targetActorIdentifier in _targetTickBuffer)
            {
                if (!_effectsByTarget.TryGetValue(targetActorIdentifier, out var list)) continue;
                if (!_world.TryGetActor(targetActorIdentifier, out var target)) continue;

                for (var i = list.Count - 1; i >= 0; i--)
                {
                    var effect = list[i];
                    effect.RemainingTime -= delta;
                    TickPeriod(effect, delta);

                    if (effect.RemainingTime > 0f) continue;
                    Remove(effect, target);
                    list.RemoveAt(i);
                }

                if (list.Count == 0)
                    _effectsByTarget.Remove(targetActorIdentifier);
            }

            _targetTickBuffer.Clear();
        }

        public void RemoveAllInvolvingActor(string actorIdentifier)
        {
            _targetTickBuffer.Clear();
            _targetTickBuffer.AddRange(_effectsByTarget.Keys);

            foreach (var targetActorIdentifier in _targetTickBuffer)
            {
                if (!_effectsByTarget.TryGetValue(targetActorIdentifier, out var list)) continue;
                if (!_world.TryGetActor(targetActorIdentifier, out var target)) continue;

                for (var i = list.Count - 1; i >= 0; i--)
                {
                    var effect = list[i];
                    if (effect.SourceActorIdentifier != actorIdentifier && effect.TargetActorIdentifier != actorIdentifier)
                        continue;

                    Remove(effect, target);
                    list.RemoveAt(i);
                }

                if (list.Count == 0)
                    _effectsByTarget.Remove(targetActorIdentifier);
            }

            _targetTickBuffer.Clear();
        }

        private List<ActiveEffect> GetList(string targetActorIdentifier)
        {
            if (!_effectsByTarget.TryGetValue(targetActorIdentifier, out var list))
            {
                list = new List<ActiveEffect>();
                _effectsByTarget[targetActorIdentifier] = list;
            }

            return list;
        }

        private void AddModifiers(ActiveEffect activeEffect, CombatActor target)
        {
            foreach (var modifier in activeEffect.Definition.AttributeModifiers)
            {
                target.Attributes.AddModifier(new AttributeModifier(
                    modifier.AttributeIdentifier,
                    modifier.Operation,
                    ResolveModifierValue(modifier, activeEffect.StackCount),
                    activeEffect));
            }
        }

        private void AddTags(ActiveEffect activeEffect, CombatActor target)
        {
            foreach (var tag in activeEffect.Definition.GrantedTags)
                _world.AddTag(target, tag);
        }

        private void RefreshModifiers(ActiveEffect activeEffect, CombatActor target)
        {
            target.Attributes.RemoveModifiersFromSource(activeEffect);
            AddModifiers(activeEffect, target);
            _world.ClampVitalAttributes(target);
        }

        private void Remove(ActiveEffect activeEffect, CombatActor target)
        {
            target.Attributes.RemoveModifiersFromSource(activeEffect);
            foreach (var tag in activeEffect.Definition.GrantedTags)
                _world.RemoveTag(target, tag);

            _world.ClampVitalAttributes(target);
            ExecuteAll(activeEffect.Definition.ExecutionsOnRemove, activeEffect.SourceActorIdentifier, activeEffect.TargetActorIdentifier);
            _world.Events.Publish(new EffectRemovedEvent
            {
                TargetActorIdentifier = activeEffect.TargetActorIdentifier,
                EffectIdentifier = activeEffect.Definition.EffectIdentifier,
            });
        }

        private void TickPeriod(ActiveEffect activeEffect, float delta)
        {
            if (activeEffect.Definition.Period <= 0f) return;

            activeEffect.PeriodTimer += delta;
            if (activeEffect.PeriodTimer < activeEffect.Definition.Period) return;

            activeEffect.PeriodTimer = 0f;
            ExecuteAll(activeEffect.Definition.ExecutionsOnPeriod, activeEffect.SourceActorIdentifier, activeEffect.TargetActorIdentifier);
        }

        private void ExecuteAll(List<EffectExecutionDefinition> executions, string sourceActorIdentifier, string targetActorIdentifier)
        {
            foreach (var execution in executions)
                Execute(execution, sourceActorIdentifier, targetActorIdentifier);
        }

        private void Execute(EffectExecutionDefinition execution, string sourceActorIdentifier, string targetActorIdentifier)
        {
            switch (execution.ExecutionType)
            {
                case EffectExecutionType.Damage:
                    _world.Damage.Resolve(new DamageRequest(sourceActorIdentifier, targetActorIdentifier, execution.Value, execution.DamageTypeTag, false));
                    break;
                case EffectExecutionType.Heal:
                    if (_world.TryGetActor(targetActorIdentifier, out var target))
                        _world.ChangeCurrentAttribute(target, CombatAttributes.Health, execution.Value, 0f, target.MaxHealth);
                    break;
                case EffectExecutionType.ApplyEffect:
                    _world.Effects.Apply(new EffectApplicationRequest(sourceActorIdentifier, targetActorIdentifier, execution.EffectIdentifier));
                    break;
                case EffectExecutionType.RequestCue:
                    var position = _world.TryGetActor(targetActorIdentifier, out var cueTarget)
                        ? cueTarget.Position
                        : CombatVector3.Zero;
                    _world.Cues.Request(new CueRequest(execution.CueIdentifier, sourceActorIdentifier, targetActorIdentifier, position));
                    break;
            }
        }

        private static float ResolveModifierValue(AttributeModifierDefinition modifier, int stackCount)
        {
            if (stackCount <= 1) return modifier.Value;
            return modifier.Operation == AttributeModifierOperation.Add
                ? modifier.Value * stackCount
                : (float)System.Math.Pow(modifier.Value, stackCount);
        }

        private void PublishApplied(EffectApplicationRequest request, int stackCount)
        {
            _world.Events.Publish(new EffectAppliedEvent
            {
                SourceActorIdentifier = request.SourceActorIdentifier,
                TargetActorIdentifier = request.TargetActorIdentifier,
                EffectIdentifier = request.EffectIdentifier,
                StackCount = stackCount,
            });
        }

        private sealed class ActiveEffect
        {
            public readonly EffectDefinition Definition;
            public readonly string SourceActorIdentifier;
            public readonly string TargetActorIdentifier;
            public float RemainingTime;
            public float PeriodTimer;
            public int StackCount = 1;

            public ActiveEffect(EffectDefinition definition, string sourceActorIdentifier, string targetActorIdentifier)
            {
                Definition = definition;
                SourceActorIdentifier = sourceActorIdentifier;
                TargetActorIdentifier = targetActorIdentifier;
                RemainingTime = definition.Duration;
            }
        }
    }
}
