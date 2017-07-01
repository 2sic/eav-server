using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Data
{
    // 2017-07-01 2dm - think this is not in use

    ///// <summary>
    ///// Typed Value
    ///// </summary>
    //public class TypedValue<T> : IValueOfDimension<T>
    //{
    //    private readonly IEnumerable<IValue> _values;
    //    private readonly T _typedContents;

    //    /// <summary>
    //    /// Constructs a new TypedValue
    //    /// </summary>
    //    public TypedValue(IEnumerable<IValue> values, T typedContents)
    //    {
    //        _values = values;
    //        _typedContents = typedContents;
    //    }

    //    public T this[int languageId] => this[new[] { languageId }];

    //    public T this[int[] languageIds]
    //    {
    //        get
    //        {
    //            // Value with Dimensions specified
    //            if (languageIds != null && languageIds.Length > 0 && _values != null)
    //            {
    //                // try match all specified Dimensions
    //                var valueHavingSpecifiedLanguages = _values.FirstOrDefault(va => languageIds.All(di => va.Languages.Select(d => d.DimensionId).Contains(di)));
    //                if (valueHavingSpecifiedLanguages != null)
    //                    try
    //                    {
    //                        return ((IValue<T>) valueHavingSpecifiedLanguages).TypedContents;
    //                    }
    //                    catch (InvalidCastException)
    //                    {
    //                    } // may occour for nullable types
    //            }

    //            // use Default
    //            return _typedContents == null ? default(T) : _typedContents;
    //        }
    //    }

    //    public T this[string languageKey] => this[new[] { languageKey }];

    //    public T this[string[] languageKeys]
    //    {
    //        get
    //        {
    //            // Value with Dimensions specified
    //            if (languageKeys != null && languageKeys.Length > 0 && _values != null)
    //            {
    //                // try match all specified Dimensions
    //                var valueHavingSpecifiedLanguages = _values.FirstOrDefault(va => languageKeys.All(lk => va.Languages.Select(d => d.Key).Contains(lk.ToLower())));
    //                if (valueHavingSpecifiedLanguages != null)
    //                    try
    //                    {
    //                        return ((IValue<T>) valueHavingSpecifiedLanguages).TypedContents;
    //                    }
    //                    catch (InvalidCastException)
    //                    {
    //                    } // may occour for nullable types
    //            }

    //            // use Default
    //            return _typedContents == null ? default(T) : _typedContents;
    //        }
    //    }
    //}
}