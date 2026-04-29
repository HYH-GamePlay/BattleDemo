using System.Collections.Generic;

namespace Combat.Runtime.Attributes
{
    public sealed class AttributeSet
    {
        private readonly Dictionary<string, float> _baseValues = new();
        private readonly Dictionary<string, float> _currentValues = new();
        private readonly List<AttributeModifier> _modifiers = new();

        public void SetBaseValue(string attributeIdentifier, float value)
        {
            _baseValues[attributeIdentifier] = value;
        }

        public float GetBaseValue(string attributeIdentifier)
            => _baseValues.TryGetValue(attributeIdentifier, out var value) ? value : 0f;

        public float GetValue(string attributeIdentifier)
        {
            var additive = 0f;
            var multiplier = 1f;

            foreach (var modifier in _modifiers)
            {
                if (modifier.AttributeIdentifier != attributeIdentifier) continue;

                if (modifier.Operation == AttributeModifierOperation.Add)
                    additive += modifier.Value;
                else
                    multiplier *= modifier.Value;
            }

            return (GetBaseValue(attributeIdentifier) + additive) * multiplier;
        }

        public void SetCurrentValue(string attributeIdentifier, float value)
        {
            _currentValues[attributeIdentifier] = value;
        }

        public float GetCurrentValue(string attributeIdentifier)
            => _currentValues.TryGetValue(attributeIdentifier, out var value)
                ? value
                : GetValue(attributeIdentifier);

        public float ChangeCurrentValue(string attributeIdentifier, float delta, float minimum, float maximum)
        {
            var value = Core.CombatMath.Clamp(GetCurrentValue(attributeIdentifier) + delta, minimum, maximum);
            _currentValues[attributeIdentifier] = value;
            return value;
        }

        public void AddModifier(AttributeModifier modifier)
        {
            _modifiers.Add(modifier);
        }

        public void RemoveModifiersFromSource(object source)
        {
            _modifiers.RemoveAll(modifier => ReferenceEquals(modifier.Source, source));
        }

        public void ClearModifiers() => _modifiers.Clear();
    }
}
