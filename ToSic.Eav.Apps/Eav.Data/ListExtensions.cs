using System;
using System.Collections.Generic;
using System.Linq;
using ListIntNull = System.Collections.Generic.List<int?>;
using DicNameInt = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<string, System.Collections.Generic.List<int?>>>;

namespace ToSic.Eav.Data;

// todo: maybe move to something like ToSic.Eav.Generics
internal static class ListExtensions
{
    internal static void EnsureListLength<T>(this List<T> listMain, int length)
    {
        if (listMain.Count < length)
            listMain.AddRange(Enumerable.Repeat(default(T), length - listMain.Count));
    }

    internal static void SetLength<T>(this List<T> list, int length)
    {
        var difference = length - list.Count;
        if (difference < 0) list.RemoveRange(length, -difference);
        else if (difference > 0) list.EnsureListLength(length);
    }

    internal static void InsertOrAppend<T>(this IList<T> list, int position, T value)
    {
        if (list.Count > position)
            list.Insert(position, value);
        else
            list.Add(value);
    }

    internal static void RemoveIfInRange<T>(this IList<T> list, int index)
    {
        if (list.Count > index) list.RemoveAt(index);
    }

    internal static bool Move<T>(this IList<T> list, int from, int to)
    {
        if (from >= list.Count)
            return false;

        var contentId = list[from];
        list.RemoveAt(from);
        list.InsertOrAppend(to, contentId);
        return true;
    }

    internal static void ForEach(this DicNameInt list, Action<ListIntNull> action)
    {
        foreach (var keyValuePair in list) action.Invoke(keyValuePair.Value);
    }

    internal static Dictionary<string, object> ToObject<T>(this Dictionary<string, T> list)
        => list.ToDictionary(x => x.Key, x => x.Value as object);
}