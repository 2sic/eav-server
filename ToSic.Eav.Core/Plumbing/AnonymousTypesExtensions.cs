using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using static System.StringComparer;

namespace ToSic.Eav.Plumbing
{
    public static class AnonymousTypesExtensions
    {
        // inspired by https://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous
        // and https://stackoverflow.com/questions/1650681/determining-whether-a-type-is-an-anonymous-type
        public static bool IsAnonymous(this object typeOrObject)
        {
            if (typeOrObject == null) return false;

            var type = typeOrObject is Type isType ? isType : typeOrObject.GetType();
            
            // Skip further checks if it's clearly a real object
            if (type.Namespace != null) return false;

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.IsGenericType && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                   && type.Attributes.HasFlag(TypeAttributes.NotPublic);
        }

        // inspired by https://stackoverflow.com/questions/3481923/in-c-sharp-convert-anonymous-type-into-key-value-array
        public static IDictionary<string, object> ObjectToDictionary(this object a, bool mutable = false, bool caseInsensitive = false)
        {
            var props = a.GetType().GetProperties();
            if (mutable)
                return caseInsensitive
                    ? props.ToDictionary(x => x.Name, x => x.GetValue(a, null), InvariantCultureIgnoreCase)
                    : props.ToDictionary(x => x.Name, x => x.GetValue(a, null));
            var dict = caseInsensitive
                ? props.ToImmutableDictionary(x => x.Name, x => x.GetValue(a, null), InvariantCultureIgnoreCase)
                : props.ToImmutableDictionary(x => x.Name, x => x.GetValue(a, null));
            return dict;
        }

        public static bool IsSimpleType(this Type type)
        {
            return
                type.IsPrimitive ||
                new Type[] {
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
}
