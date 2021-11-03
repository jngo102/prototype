using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
public class TypePopupLabelAttribute : Attribute
{
    public string Label { get; }

    public TypePopupLabelAttribute(string label)
    {
        Label = label;
    }
}