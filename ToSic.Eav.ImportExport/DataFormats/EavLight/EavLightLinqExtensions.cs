using System;
using System.Collections.Generic;

namespace ToSic.Eav.DataFormats.EavLight;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class EavLightLinqExtensions
{
    /// <summary>
    /// Helper method to create JsonV0 from Linq - copied & Modified from .net Framework code
    /// </summary>
    /// <remarks>
    /// got it from https://github.com/dotnet/runtime/blob/57bfe474518ab5b7cfe6bf7424a79ce3af9d6657/src/libraries/System.Linq/src/System/Linq/ToCollection.cs
    /// </remarks>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="source"></param>
    /// <param name="keySelector"></param>
    /// <returns></returns>
    public static EavLightEntity ToEavLight<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

        if (source is ICollection<TSource> collection)
        {
            if (collection.Count == 0) return new();

            if (collection is TSource[] array) return ToEavLight(array, keySelector);

            if (collection is List<TSource> list) return ToEavLight(list, keySelector);
        }

        var d = new EavLightEntity();
        foreach (var element in source) d.Add(keySelector(element), element);
        return d;
    }

    private static EavLightEntity ToEavLight<TSource>(TSource[] source, Func<TSource, string> keySelector)
    {
        var d = new EavLightEntity();
        for (var i = 0; i < source.Length; i++) d.Add(keySelector(source[i]), source[i]);
        return d;
    }

    private static EavLightEntity ToEavLight<TSource>(List<TSource> source, Func<TSource, string> keySelector)
    {
        var d = new EavLightEntity();
        foreach (var element in source) d.Add(keySelector(element), element);

        return d;
    }


    public static EavLightEntity ToEavLight<TSource, TElement>(this IEnumerable<TSource> source, Func<TSource, string> keySelector, Func<TSource, TElement> elementSelector) 
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
        if (elementSelector == null) throw new ArgumentNullException(nameof(elementSelector));

        if (source is ICollection<TSource> collection)
        {
            if (collection.Count == 0)
                return new();

            if (collection is TSource[] array) return ToEavLight(array, keySelector, elementSelector);

            if (collection is List<TSource> list) return ToEavLight(list, keySelector, elementSelector);
        }

        var d = new EavLightEntity();
        foreach (var element in source) d.Add(keySelector(element), elementSelector(element));
        return d;
    }

    private static EavLightEntity ToEavLight<TSource, TElement>(TSource[] source, Func<TSource, string> keySelector, Func<TSource, TElement> elementSelector)
    {
        var d = new EavLightEntity();
        for (int i = 0; i < source.Length; i++) d.Add(keySelector(source[i]), elementSelector(source[i]));
        return d;
    }

    private static EavLightEntity ToEavLight<TSource, TElement>(List<TSource> source, Func<TSource, string> keySelector, Func<TSource, TElement> elementSelector)
    {
        var d = new EavLightEntity();
        foreach (var element in source) d.Add(keySelector(element), elementSelector(element));
        return d;
    }
}