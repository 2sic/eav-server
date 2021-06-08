using System;

namespace ToSic.Eav.Plumbing
{
    public static class ObjectExtensions
    {
        // https://stackoverflow.com/questions/6553183/check-to-see-if-a-given-object-reference-or-value-type-is-equal-to-its-default
        // //Adapted from https://stackoverflow.com/a/6553276/1889720
        public static bool IsNullOrDefault<TObject>(this TObject argument, bool treatFalseAsDefault = true)
        {
            // deal with normal scenarios
            if (argument == null) return true;
            if (object.Equals(argument, default(TObject))) return true;

            // deal with non-null nullables
            var methodType = typeof(TObject);
            if (Nullable.GetUnderlyingType(methodType) != null) return false;

            // 2dm: Treat boolean false as a valid value, not as default
            if (!treatFalseAsDefault && argument is bool) 
                return false;
            
            // deal with boxed value types
            var argumentType = argument.GetType();
            if (argumentType.IsValueType && argumentType != methodType)
            {
                var obj = Activator.CreateInstance(argument.GetType());
                return obj.Equals(argument);
            }

            return false;
        }
    }
}
