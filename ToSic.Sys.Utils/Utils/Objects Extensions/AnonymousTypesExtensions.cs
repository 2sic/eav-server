using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using static System.StringComparer;

namespace ToSic.Eav.Plumbing;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class AnonymousTypesExtensions
{
    // inspired by https://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous
    // and https://stackoverflow.com/questions/1650681/determining-whether-a-type-is-an-anonymous-type
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool IsAnonymous(this object? typeOrObject)
    {
        if (typeOrObject == null)
            return false;

        var type = typeOrObject is Type isType ? isType : typeOrObject.GetType();
            
        // Skip further checks if it's clearly a real object
        if (type.Namespace != null)
            return false;

        // HACK: The only way to detect anonymous types right now.
        return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
               && type.IsGenericType && type.Name.Contains("AnonymousType")
               && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
               && type.Attributes.HasFlag(TypeAttributes.NotPublic);
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IDictionary<string, object?> ToDicInvariantInsensitive(this object a, bool mutable = false)
        => ObjectToDictionary(a, mutable, caseInsensitive: true);

    // inspired by https://stackoverflow.com/questions/3481923/in-c-sharp-convert-anonymous-type-into-key-value-array
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static IDictionary<string, object?> ObjectToDictionary(this object a, bool mutable = false, bool caseInsensitive = false)
    {
        // edge case for object that is 'string'
        // prevent exception "Parameter count mismatch" in x.GetValue(a, null) for object that is 'string'
        if (a is string) 
        {
            var d = new Dictionary<string, object?>(caseInsensitive ? InvariantCultureIgnoreCase : null) { { "Value", a } };
            return mutable
                ? d
                : d.ToImmutableDictionary(caseInsensitive ? InvariantCultureIgnoreCase : null);
        }
        
        var props = a.GetType().GetProperties();

#pragma warning disable CS8619
        if (mutable)
            return caseInsensitive
                ? props.ToDictionary(x => x.Name, x => x.GetValue(a, null), InvariantCultureIgnoreCase)
                : props.ToDictionary(x => x.Name, x => x.GetValue(a, null));
        return caseInsensitive
            ? props.ToImmutableDictionary(x => x.Name, x => x.GetValue(a, null), InvariantCultureIgnoreCase)
            : props.ToImmutableDictionary(x => x.Name, x => x.GetValue(a, null));
#pragma warning restore CS8619
    }

    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool IsSimpleType(this Type type)
    {
        return
            type.IsPrimitive ||
            new[] {
                typeof(string),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
            }.Contains(type) ||
            type.IsEnum ||
            // Nullable
            (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsSimpleType(type.GetGenericArguments()[0])) ||
            // Specific object - but must check for anonymous object
            Convert.GetTypeCode(type) != TypeCode.Object
            ;
    }
}