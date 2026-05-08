using System;

namespace Battle.Core
{
    public readonly struct BehaviorHandle : IEquatable<BehaviorHandle>
    {
        public static readonly BehaviorHandle Invalid = new BehaviorHandle(0);

        public BehaviorHandle(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public bool IsValid => Value > 0;

        public bool Equals(BehaviorHandle other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is BehaviorHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return IsValid ? Value.ToString() : "Invalid";
        }

        public static bool operator ==(BehaviorHandle left, BehaviorHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BehaviorHandle left, BehaviorHandle right)
        {
            return !left.Equals(right);
        }
    }
}
