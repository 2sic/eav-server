using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Testing.Shared;

/// <summary>
/// Special extension methods to convert lists of objects to a property for use in a [DynamicData] attribute.
/// </summary>
public static class TestDataExtensions
{
    /// <summary>
    /// Simple convert-to IEnumerable of object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static IEnumerable<object[]> ToTestEnum<T>(this IEnumerable<T> list)
        => list.Select(bk => new object[] { bk }).ToList();

    /// <summary>
    /// Convert to IEnumerable / object, with a filtering condition first.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="condition"></param>
    /// <returns></returns>
    public static IEnumerable<object[]> ToTestEnum<T>(this IEnumerable<T> list, Func<T, bool> condition)
        => list.Where(condition).ToList().ToTestEnum();
}