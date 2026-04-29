namespace Combat.Runtime.Attributes
{
    public enum AttributeModifierOperation
    {
        Add,
        Multiply,
    }

    public readonly struct AttributeModifier
    {
        public readonly string AttributeIdentifier;
        public readonly AttributeModifierOperation Operation;
        public readonly float Value;
        public readonly object Source;

        public AttributeModifier(string attributeIdentifier, AttributeModifierOperation operation, float value, object source)
        {
            AttributeIdentifier = attributeIdentifier;
            Operation = operation;
            Value = value;
            Source = source;
        }
    }
}
