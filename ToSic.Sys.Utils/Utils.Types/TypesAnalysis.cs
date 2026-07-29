namespace ToSic.Sys.Utils.Types;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class TypeAnalysis
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool IsSimpleType(this Type type) =>
        type.IsPrimitive ||
        type.IsEnum ||
        SimpleTypes.Contains(type) ||
        // Nullable
        (type.IsGenericTypeOf(typeof(Nullable<>)) && type.GetGenericArguments()[0].IsSimpleType()) ||
        // Specific object - but must check for anonymous object
        Convert.GetTypeCode(type) != TypeCode.Object;

    private static readonly Type[] SimpleTypes =
    [
        typeof(string),
        typeof(decimal),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(Guid)
    ];
}