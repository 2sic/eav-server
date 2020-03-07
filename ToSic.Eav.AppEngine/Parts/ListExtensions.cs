using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Apps.Parts
{
    // todo: maybe move to something like ToSic.Eav.Generics
    internal static class ListExtensions
    {
        internal static void EnsureListLength<T>(this List<T> listMain, int sortOrder)
        {
            if (listMain.Count < sortOrder + 1)
                listMain.AddRange(Enumerable.Repeat(default(T), (sortOrder + 1) - listMain.Count));
        }

        internal static void InsertOrAppend<T>(this IList<T> list, int position, T value)
        {
            if (list.Count > position)
                list.Insert(position, value);
            else
                list.Add(value);
        }
    }
}
