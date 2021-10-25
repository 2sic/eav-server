using System;
using System.Reflection;
using System.Runtime.CompilerServices;

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
    }
}
