namespace ToSic.Eav.Data;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ValueTypeHelpers
{
    /// <summary>
    /// Look up the type code if we started with a string.
    /// This is case-insensitive.
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static ValueTypes Get(string typeName) 
        => Enum.TryParse<ValueTypes>(typeName ?? "", true, out var code) ? code : ValueTypes.Undefined;

    public static Dictionary<ValueTypes, Type> TypeMap = new()
    {
        {ValueTypes.Boolean, typeof(bool)},
        {ValueTypes.DateTime, typeof(DateTime)},
        {ValueTypes.Entity, typeof(int)},
        {ValueTypes.Hyperlink, typeof(string)},
        {ValueTypes.Number, typeof(double)},
        {ValueTypes.String, typeof(string)},
        {ValueTypes.Empty, typeof(string)},
        {ValueTypes.Custom, typeof(string)},
        {ValueTypes.Json, typeof(string)}
    };

    public static Type GetType(ValueTypes type) => TypeMap[type];
}