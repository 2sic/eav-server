using System.Globalization;

namespace ToSic.Eav.Plumbing;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static partial class ObjectExtensions
{
    // https://stackoverflow.com/questions/6553183/check-to-see-if-a-given-object-reference-or-value-type-is-equal-to-its-default
    // //Adapted from https://stackoverflow.com/a/6553276/1889720
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool IsNullOrDefault<TObject>(this TObject argument, bool boolIsNeverDefault = true)
    {
        // deal with normal scenarios
        // todo: check if == is ok, or if we should use "is"
        if (argument == null)
            return true;
        if (object.Equals(argument, default(TObject)))
            return true;

        // deal with non-null nullables
        var methodType = typeof(TObject);
        if (Nullable.GetUnderlyingType(methodType) != null)
            return false;

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
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static T? ConvertOrDefault<T>(this object value, bool numeric = false, bool truthy = false) 
        => value.TryConvert<T>(numeric: numeric, truthy: truthy).Value;

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static (bool Success, T? Value) TryConvert<T>(this object? value, bool numeric = false, bool truthy = false)
    {
        if (value is null)
            return (false, default);
        // 2023-08-18 2dm: Added minor optimization; remove comment by EOY if no problems
        if (value is T alreadyTyped) return (true, alreadyTyped);

        var t = typeof(T);
        var unboxedT = t.UnboxIfNullable();

        // Some pre-checks which we'll need anyhow, and should result in less try/catch
        var toBool = unboxedT == typeof(bool);
        var toNumber = unboxedT.IsNumeric();

        // If we start with a string, we can optimize number and boolean conversion
        // Numbers are first converted to decimal, because it could handle .notation
        // Booleans are also first converted to decimal, so that "1.1" or "27" is treated as truthy
        if (value is string s)
        {
            // Short circuit string-to-string
            if (unboxedT == typeof(string)) return (true, (T)(object)s);

            // Catch empty strings early for very common use cases
            if (string.IsNullOrWhiteSpace(s) && (toBool || toNumber)) 
                return (false, default);

            if (numeric && toNumber || truthy && toBool)
            {
                if (s.IndexOf(',') > -1) s = s.Replace(',', '.');
                if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                    value = dec;
            }
            // Strings cannot convert to guids with Convert.ChangeType, so we do extra treatment if possible
            // Note that if this fails, it will run the normal code resulting in a Guid.Empty
            else if (unboxedT == typeof(Guid) && Guid.TryParse(s, out var guid)) 
                return (true, (T)(object)guid);
        }


        try
        {
            var result = (T)Convert.ChangeType(value, unboxedT, CultureInfo.InvariantCulture);
            return (true, result);
        }
        catch
        {
            return (false, default);
        }
    }



    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static T ConvertOrFallback<T>(this object? value, T fallback, bool numeric = false, bool truthy = false, bool fallbackOnDefault = false)
    {
        if (value is null)
            return fallback;
        try
        {
            var result = value.TryConvert<T>(numeric: numeric, truthy: truthy);
            // Null should always fallback, default not always
            if (!result.Success || result.Value == null)
                return fallback;
            if (fallbackOnDefault && IsNullOrDefault(result))
                return fallback;
            return result.Value;
        }
        catch
        {
            return fallback;
        }
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool IsDefault<T>(this T? value)
        => EqualityComparer<T>.Default.Equals(value, default);

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static bool IsNotDefault<T>(this T? value)
        => !EqualityComparer<T>.Default.Equals(value, default);
}