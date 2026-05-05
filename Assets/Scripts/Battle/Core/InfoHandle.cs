using System;

namespace Battle.Core
{
    public readonly struct InfoHandle : IEquatable<InfoHandle>
    {
        public static readonly InfoHandle Invalid = new InfoHandle(0);

        public InfoHandle(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public bool IsValid => Value > 0;

        public bool Equals(InfoHandle other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is InfoHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return IsValid ? Value.ToString() : "Invalid";
        }

        public static bool operator ==(InfoHandle left, InfoHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InfoHandle left, InfoHandle right)
        {
            return !left.Equals(right);
        }
    }
}
