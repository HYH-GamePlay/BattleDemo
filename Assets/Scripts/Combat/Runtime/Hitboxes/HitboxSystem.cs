using System.Collections.Generic;
using Combat.Runtime.Core;
using Combat.Runtime.Cues;
using Combat.Runtime.Damage;
using Combat.Runtime.Effects;

namespace Combat.Runtime.Hitboxes
{
    public sealed class HitboxSystem
    {
        private readonly CombatWorld _world;
        private readonly List<Actors.CombatActor> _candidateBuffer = new();

        public HitboxSystem(CombatWorld world)
        {
            _world = world;
        }

        public void Activate(string hitboxIdentifier, string sourceActorIdentifier)
        {
            if (!_world.DataRegistry.TryGetHitbox(hitboxIdentifier, out var definition)) return;
            if (!_world.TryGetActor(sourceActorIdentifier, out var source)) return;

            var center = source.Position + definition.Offset;
            var radiusSquared = definition.Radius * definition.Radius;

            _candidateBuffer.Clear();
            _candidateBuffer.AddRange(_world.Actors);

            foreach (var candidate in _candidateBuffer)
            {
                if (!CanHit(definition, source, candidate)) continue;
                if (CombatVector3.DistanceSquared(center, candidate.Position) > radiusSquared) continue;

                if (definition.Damage > 0f)
                    _world.Damage.Resolve(new DamageRequest(source.ActorIdentifier, candidate.ActorIdentifier, definition.Damage, definition.DamageTypeTag));

                if (!string.IsNullOrEmpty(definition.EffectIdentifier))
                    _world.Effects.Apply(new EffectApplicationRequest(source.ActorIdentifier, candidate.ActorIdentifier, definition.EffectIdentifier));

                if (!string.IsNullOrEmpty(definition.CueIdentifier))
                    _world.Cues.Request(new CueRequest(definition.CueIdentifier, source.ActorIdentifier, candidate.ActorIdentifier, candidate.Position));
            }

            _candidateBuffer.Clear();
        }

        private static bool CanHit(HitboxDefinition definition, Actors.CombatActor source, Actors.CombatActor target)
        {
            if (target == null || target.IsDead) return false;
            if (ReferenceEquals(source, target)) return definition.HitSelf;

            var sameTeam = source.TeamIdentifier == target.TeamIdentifier;
            return sameTeam ? definition.HitAllies : definition.HitEnemies;
        }
    }
}
