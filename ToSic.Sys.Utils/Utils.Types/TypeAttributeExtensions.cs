namespace ToSic.Sys.Utils.Types;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class TypeAttributeExtensions
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static T? GetDirectlyAttachedAttribute<T>(this Type type) where T : class
        => type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;

}