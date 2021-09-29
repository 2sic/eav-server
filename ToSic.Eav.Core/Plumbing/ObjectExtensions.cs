using System;
using System.Globalization;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Plumbing
{
    public static class ObjectExtensions
    {
        // https://stackoverflow.com/questions/6553183/check-to-see-if-a-given-object-reference-or-value-type-is-equal-to-its-default
        // //Adapted from https://stackoverflow.com/a/6553276/1889720
        public static bool IsNullOrDefault<TObject>(this TObject argument, bool boolIsNeverDefault = true)
        {
            // deal with normal scenarios
            // todo: check if == is ok, or if we should use "is"
            if (argument == null) return true;
            if (object.Equals(argument, default(TObject))) return true;

            // deal with non-null nullables
            var methodType = typeof(TObject);
            if (Nullable.GetUnderlyingType(methodType) != null) return false;

            // 2dm: Treat boolean false as a valid value, not as default
            if (boolIsNeverDefault && argument is bool) 
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

        /// <summary>
        /// Will try to convert an object to a type, or if not valid
        /// return the default (null, zero, etc.) of a type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks>
        /// Used in EntityLight, Entity and soon also by DynamicCode // by Entity.cs, because that uses it's own GetBestValue(...)
        /// </remarks>
        [PrivateApi]
        public static T ConvertOrDefault<T>(this object value, bool numeric = false, bool truthy = false) 
            => value.TryConvert<T>(numeric: numeric, truthy: truthy).Item2;

        public static Tuple<bool, T> TryConvert<T>(this object value, bool numeric = false, bool truthy = false)
        {
            if (value is null) return new Tuple<bool, T>(false, default);
            var t = typeof(T);
            var unboxedT = t.UnboxIfNullable();

            // If we start with a string, we can optimize number and boolean conversion
            // Numbers are first converted to decimal, because it could handle .notation
            // Booleans are also first converted to decimal, so that "1.1" or "27" is treated as truthy
            if(value is string s)
                if (((numeric && unboxedT.IsNumeric()) || (truthy && unboxedT == typeof(bool))))
                {
                    if (s.IndexOf(',') > -1) s = s.Replace(',', '.');
                    if (decimal.TryParse(s, out var dec))
                        value = dec;
                }


            try
            {
                var result = (T)Convert.ChangeType(value, unboxedT, CultureInfo.InvariantCulture);
                return new Tuple<bool, T>(true, result);
            }
            catch
            {
                return new Tuple<bool, T>(false, default);
            }
        }



        public static T ConvertOrFallback<T>(this object value, T fallback, bool numeric = false, bool truthy = false, bool fallbackOnDefault = false)
        {
            if (value is null) return fallback;
            try
            {
                var valToConvert = value;
                var result = valToConvert.TryConvert<T>(numeric: numeric, truthy: truthy);
                // Null should always fallback, default not always
                if (!result.Item1 || result.Item2 == null) return fallback;
                if (fallbackOnDefault && IsNullOrDefault(result)) return fallback;
                return result.Item2;
            }
            catch
            {
                return fallback;
            }
        }
    }
}
