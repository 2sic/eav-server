namespace ToSic.Sys.Utils;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class TypeExtensions
{
    /// <summary>
    /// If it's a nullable type (like int?) then get the unboxed type (like int)
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static Type UnboxIfNullable(this Type t)
        => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)
            ? Nullable.GetUnderlyingType(t) ?? t
            : t;

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool IsNumeric(this object? o)
        => o is not null && o.GetType().IsNumeric();

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool IsNumeric(this Type t)
        => Type.GetTypeCode(t) switch
        {
            TypeCode.Byte => true,
            TypeCode.SByte => true,
            TypeCode.UInt16 => true,
            TypeCode.UInt32 => true,
            TypeCode.UInt64 => true,
            TypeCode.Int16 => true,
            TypeCode.Int32 => true,
            TypeCode.Int64 => true,
            TypeCode.Decimal => true,
            TypeCode.Double => true,
            TypeCode.Single => true,
            _ => false
        };

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static T? GetDirectlyAttachedAttribute<T>(this Type type) where T : class
        => type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;
}