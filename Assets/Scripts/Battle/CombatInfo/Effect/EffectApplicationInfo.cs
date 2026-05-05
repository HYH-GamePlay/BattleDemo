using System.Collections.Generic;
using Battle.Ability.Effects;

namespace Battle.CombatInfo.Effect
{
    public sealed class EffectApplicationInfo : ICombatInfo
    {
        private readonly List<EffectSpec> _pendingSpecs = new List<EffectSpec>();

        public IReadOnlyList<EffectSpec> PendingSpecs => _pendingSpecs;

        public void Add(EffectSpec spec)
        {
            if (spec != null)
            {
                _pendingSpecs.Add(spec);
            }
        }

        public void Clear()
        {
            _pendingSpecs.Clear();
        }
    }
}
