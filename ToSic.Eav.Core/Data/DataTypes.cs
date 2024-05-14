using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data;

/// <summary>
/// Constants for data types
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class DataTypes
{
    public const string Boolean = "Boolean";
    public const string Number = "Number";
    public const string DateTime = "DateTime";
    public const string Entity = "Entity"; // todo: update all references with this as a constant
    public const string Hyperlink = "Hyperlink";
    public const string String = "String";
        
    // Don't call this "Empty" because it's too similar to "Entity" and could be overlooked when coding
    public static string VoidEmpty = "Empty";

    // TODO: Also look for other code which does very similar stuff, and try to de-duplicate
    // helper to get text-name of the type
    public static ValueTypes GetAttributeTypeName(object value)
    {
        if (value is DateTime)
            return ValueTypes.DateTime;
        if (value.IsNumeric()) // 2021-11-16 2dm changed, because it missed bigint from SQL - original was: // (value is decimal || value is int || value is double)
            return ValueTypes.Number;
        if (value is bool)
            return ValueTypes.Boolean;
        if (value is Guid || value is List<Guid> || value is List<Guid?> || value is List<int> ||
            value is List<int?>)
            return ValueTypes.Entity;
        if (value is int[] || value is int?[])
            throw new(
                "Trying to provide an attribute with a value which is an int-array. This is not allowed - ask the iJungleboy.");
        return ValueTypes.String;
    }

}