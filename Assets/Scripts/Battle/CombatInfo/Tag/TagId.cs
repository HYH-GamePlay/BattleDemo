using System;

namespace Battle.CombatInfo.Tag
{
    public readonly struct TagId : IEquatable<TagId>
    {
        public static readonly TagId Invalid = new TagId(0);

        public TagId(int value)
        {
            Value = value;
        }

        public int Value { get; }

        public bool IsValid => Value > 0;

        public bool Equals(TagId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is TagId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return IsValid ? Value.ToString() : "Invalid";
        }

        public static bool operator ==(TagId left, TagId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TagId left, TagId right)
        {
            return !left.Equals(right);
        }
    }
}
