using System.Collections.Immutable;

namespace ToSic.Eav.Data;

/// <summary>
/// Represents an Attribute / Property of an Entity with Values of a Generic Type.
/// </summary>
/// <remarks>
/// * completely #immutable since v15.04
/// * We recommend you read about the [](xref:Basics.Data.Index)
/// </remarks>
/// <typeparam name="T">Type of the Value</typeparam>
[PrivateApi("Hidden in 12.04 2021-09 because people should only use the interface - previously InternalApi, this is just fyi, use interface IAttribute<T>")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
internal class Attribute<T>(string name, ValueTypes type, IImmutableList<IValue> values = null)
    : AttributeBase(name, type), IAttribute<T>
{
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public IAttribute CloneWithNewValues(IImmutableList<IValue> values)
        => new Attribute<T>(Name, Type, values);

    /// <inheritdoc/>
    public IEnumerable<IValue> Values => MyValues;

    /// <summary>
    /// Private immutable values - never null - for direct access & better performance.
    /// </summary>
    private IImmutableList<IValue> MyValues { get; } = values ?? EmptyValues;


    /// <inheritdoc/>
    public T TypedContents
    {
        get
        {
            try
            {
                var value = GetTypedValue();
                return value != null ? value.TypedContents : default;
            }
            catch
            {
                return default;
            }
        }
    }

    internal IValue<T> GetTypedValue()
    {
        try
        {
            // in some cases Values can be null
            return MyValues.FirstOrDefault() as IValue<T>;
        }
        catch
        {
            return default;
        }
    }

    /// <inheritdoc/>
    public IEnumerable<IValue<T>> Typed
        => MyValues.Cast<IValue<T>>().ToList();

    // 2024-06-04 2dm disabled completely, cannot imagine anybody using this on the Typed-Interface - clean-up 2024-Q3
    ///// <inheritdoc/>
    //[PrivateApi]
    //[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    //T IAttribute<T>.this[int languageId]
    //    => GetInternal([languageId], IsDefault, FindHavingDimensions);

    #region IAttribute Implementations
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    object IAttribute.this[string languageKey]
        => GetInternal([languageKey.ToLowerInvariant(), null], IsDefault, FindHavingDimensionsLowerCase, fallbackToAny: true);


    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public (IValue ValueField, object Result) GetTypedValue(string[] languageKeys, bool fallbackToAny)
    {
        var iVal = GetInternalValue(languageKeys, IsDefault, FindHavingDimensionsLowerCase, fallbackToAny: fallbackToAny);
        return (iVal, iVal == null ? default : iVal.TypedContents);
    }

    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    object IAttribute.this[int languageId]
        => GetInternal([languageId], IsDefault, FindHavingDimensions, fallbackToAny: true);
    #endregion

    private T GetInternal<TKey>(TKey[] keys, Func<TKey, bool> isDefault, Func<TKey[], IValue> lookupCallback, bool fallbackToAny)
    {
        var valT = GetInternalValue(keys, isDefault, lookupCallback, fallbackToAny);
        return valT == null ? default : valT.TypedContents;
    }

    // 2024-06-04 2dm changing to use IsDefault methods instead of generic checks (faster)
    //private T GetInternal<TKey>(TKey[] keys, Func<TKey[], IValue> lookupCallback)
    //{
    //    var valT = GetInternalValue(keys, lookupCallback);
    //    return valT == null ? default : valT.TypedContents;
    //}


    //private IValue<T> GetInternalValue<TKey>(TKey[] keys, Func<TKey[], IValue> lookupCallback)
    //{
    //    // no values, exit early, return default
    //    if (MyValues.Count == 0) return default;

    //    // If no keys, return first value
    //    if (keys is not { Length: > 0 }) return GetTypedValue();

    //    // Value with Dimensions specified
    //    // try match all specified Dimensions
    //    // note that as of now, the dimensions are always just 1 language, not more
    //    // so the dimensions are _not_ a list of languages, but would contain other dimensions
    //    // that is why we match ALL - but in truth it's a "feature" that's never been used
    //    foreach (var key in keys)
    //    {
    //        IValue valueHavingSpecifiedLanguages;
    //        // if it's null or 0, try to just get anything
    //        if (EqualityComparer<TKey>.Default.Equals(key, default))
    //            valueHavingSpecifiedLanguages = MyValues.FirstOrDefault();
    //        else if (key != null)
    //            valueHavingSpecifiedLanguages = lookupCallback([key]);
    //        else
    //            continue;

                
    //        if (valueHavingSpecifiedLanguages == null) continue;

    //        // stop at first non-null match
    //        try
    //        {
    //            return (IValue<T>)valueHavingSpecifiedLanguages;
    //        }
    //        catch (InvalidCastException) { /* ignore, may occur for nullable types */ }
    //        break;
    //    }

    //    // Fallback to use Default
    //    return GetTypedValue();
    //}

    private IValue<T> GetInternalValue<TKey>(TKey[] keys, Func<TKey, bool> isDefault, Func<TKey[], IValue> lookupCallback, bool fallbackToAny)
    {
        // no values, exit early, return default
        if (MyValues.Count == 0) return default;

        // If no keys, return first value
        if (keys is not { Length: > 0 }) return fallbackToAny ? GetTypedValue() : default;

        // Value with Dimensions specified
        // try match all specified Dimensions
        // note that as of now, the dimensions are always just 1 language, not more
        // so the dimensions are _not_ a list of languages, but would contain other dimensions
        // that is why we match ALL - but in truth it's a "feature" that's never been used
        foreach (var key in keys)
        {
            IValue valueHavingSpecifiedLanguages;
            // if it's null or 0, try to just get anything
            if (isDefault(key))
                valueHavingSpecifiedLanguages = GetTypedValue();
            else if (key != null)
                valueHavingSpecifiedLanguages = lookupCallback([key]);
            else
                continue;


            if (valueHavingSpecifiedLanguages == null) continue;

            // stop at first non-null match
            //try
            //{
                return valueHavingSpecifiedLanguages as IValue<T>;
            //}
            //catch (InvalidCastException) { /* ignore, may occur for nullable types */ }
            break;
        }

        // Fallback to use Default
        return fallbackToAny ? GetTypedValue() : default;
    }

    private static bool IsDefault(string key) => key == default;

    private static bool IsDefault(int key) => key == default;

    private IValue FindHavingDimensions(int[] keys)
    {
        var valuesHavingDimensions = MyValues
            .FirstOrDefault(va => keys.All(di => va.Languages.Select(d => d.DimensionId).Contains(di)));
        return valuesHavingDimensions;
    }

    //private IValue FindHavingDimensions(string[] keys)
    //{
    //    // ensure language Keys in lookup-list are lowered
    //    var langsLower = keys.Select(l => l.ToLowerInvariant()).ToArray();
    //    var valuesHavingDimensions = MyValues
    //        .FirstOrDefault(va => langsLower.All(lng => va.Languages.Select(d => d.Key).Contains(lng)));
    //    return valuesHavingDimensions;
    //}

    /// <summary>
    /// Find the values. For performance, it requires the keys to already be lower cased. 
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    private IValue FindHavingDimensionsLowerCase(string[] keys)
    {
        // ensure language Keys in lookup-list are lowered
        var valuesHavingDimensions = MyValues
            .FirstOrDefault(va => keys.All(lng => va.Languages.Select(d => d.Key).Contains(lng)));
        return valuesHavingDimensions;
    }

    #region ToString to improve debugging experience

    public override string ToString() => $"[{GetType()}:{MyValues.Count}x] - first={GetTypedValue()?.Serialized}";

    #endregion
}