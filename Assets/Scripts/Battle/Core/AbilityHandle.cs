using System;

namespace Battle.Core
{
    public readonly struct AbilityHandle : IEquatable<AbilityHandle>
    {
        public static readonly AbilityHandle Invalid = new AbilityHandle(0);

        public AbilityHandle(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public bool IsValid => Value > 0;

        public bool Equals(AbilityHandle other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is AbilityHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return IsValid ? Value.ToString() : "Invalid";
        }

        public static bool operator ==(AbilityHandle left, AbilityHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AbilityHandle left, AbilityHandle right)
        {
            return !left.Equals(right);
        }
    }
}
