using System;

namespace Battle.Core
{
    public readonly struct ActorId : IEquatable<ActorId>
    {
        public static readonly ActorId Invalid = new ActorId(0);

        public ActorId(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public bool IsValid => Value > 0;

        public bool Equals(ActorId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is ActorId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return IsValid ? Value.ToString() : "Invalid";
        }

        public static bool operator ==(ActorId left, ActorId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ActorId left, ActorId right)
        {
            return !left.Equals(right);
        }
    }
}
