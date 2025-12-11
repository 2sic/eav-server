namespace ToSic.Eav.Data.Sys;

/// <summary>
/// Constants for data types
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class DataTypes
{
    public static readonly string Boolean = nameof(ValueTypes.Boolean);// "Boolean";
    public static readonly string Number = nameof(ValueTypes.Number);
    public static readonly string DateTime = nameof(ValueTypes.DateTime);
    //public const string Entity = "Entity"; // todo: update all references with this as a constant
    public static readonly string Hyperlink = nameof(ValueTypes.Hyperlink);
    public static readonly string String = nameof(ValueTypes.String);

    // Don't call this "Empty" because it's too similar to "Entity" and could be overlooked when coding
    //public static string VoidEmpty = "Empty";

    // TODO: Also look for other code which does very similar stuff, and try to de-duplicate
    // helper to get text-name of the type
    public static ValueTypes GetAttributeTypeName(object? value, bool allowUnknownValueTypes)
    {
        if (value is DateTime)
            return ValueTypes.DateTime;
        if (value.IsNumeric()) // 2021-11-16 2dm changed, because it missed bigint from SQL - original was: // (value is decimal || value is int || value is double)
            return ValueTypes.Number;
        if (value is bool)
            return ValueTypes.Boolean;
        // 2024-08-28 2bf disabled Guid because external DataSource may have Guids and 2sxc uses Guid only in lists
        if (value is ICollection<Guid> or ICollection<Guid?> or ICollection<int> or ICollection<int?>)
            return ValueTypes.Entity;
        if (value is int[] or int?[])
            throw new(
                "Trying to provide an attribute with a value which is an int-array. This is not allowed - ask the iJungleboy.");

        if (allowUnknownValueTypes && value is not string)
            return ValueTypes.Object;

        return ValueTypes.String;
    }

}