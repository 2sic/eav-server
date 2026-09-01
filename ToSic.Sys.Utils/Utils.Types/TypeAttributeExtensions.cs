namespace ToSic.Sys.Utils.Types;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class TypeAttributeExtensions
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static T? GetDirectlyAttachedAttribute<T>(this Type type) where T : class
        => type.GetCustomAttributes(typeof(T), false).FirstOrDefault() as T;

    public static List<(Type Type, T Attribute)> GetTypesWithAttribute<T>(this System.Reflection.Assembly assembly)
        where T : Attribute
        => assembly
            .GetTypes()
            .Select(type => (Type: type, Attribute: type.GetDirectlyAttachedAttribute<T>()!))
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            .Where(pair => pair.Attribute != null)
            .ToList();
}