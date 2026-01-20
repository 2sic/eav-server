namespace ToSic.Eav.Data;

public static class AttributeTestAccessors
{
    public static IEnumerable<IValue> ValuesTac(this IAttribute attribute)
        => attribute.Values;
}