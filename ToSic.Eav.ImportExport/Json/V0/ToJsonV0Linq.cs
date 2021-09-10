using System;
using System.Collections.Generic;

namespace ToSic.Eav.ImportExport.Json.V0
{
    public static class ToJsonV0Linq
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
        public static IJsonEntity ToJsonV0<TSource>(this IEnumerable<TSource> source, Func<TSource, string> keySelector)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            if (source is ICollection<TSource> collection)
            {
                if (collection.Count == 0) return new JsonEntity();

                if (collection is TSource[] array) return ToJsonV0(array, keySelector);

                if (collection is List<TSource> list) return ToJsonV0(list, keySelector);
            }

            var d = new JsonEntity();
            foreach (var element in source) d.Add(keySelector(element), element);
            return d;
        }

        private static IJsonEntity ToJsonV0<TSource>(TSource[] source, Func<TSource, string> keySelector)
        {
            var d = new JsonEntity();
            for (var i = 0; i < source.Length; i++) d.Add(keySelector(source[i]), source[i]);
            return d;
        }

        private static IJsonEntity ToJsonV0<TSource>(List<TSource> source, Func<TSource, string> keySelector)
        {
            var d = new JsonEntity();
            foreach (var element in source) d.Add(keySelector(element), element);

            return d;
        }


        public static IJsonEntity ToJsonV0<TSource, TElement>(this IEnumerable<TSource> source, Func<TSource, string> keySelector, Func<TSource, TElement> elementSelector) 
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (elementSelector == null) throw new ArgumentNullException(nameof(elementSelector));

            if (source is ICollection<TSource> collection)
            {
                if (collection.Count == 0)
                    return new JsonEntity();

                if (collection is TSource[] array) return ToJsonV0(array, keySelector, elementSelector);

                if (collection is List<TSource> list) return ToJsonV0(list, keySelector, elementSelector);
            }

            var d = new JsonEntity();
            foreach (var element in source) d.Add(keySelector(element), elementSelector(element));
            return d;
        }

        private static IJsonEntity ToJsonV0<TSource, TElement>(TSource[] source, Func<TSource, string> keySelector, Func<TSource, TElement> elementSelector)
        {
            var d = new JsonEntity();
            for (int i = 0; i < source.Length; i++) d.Add(keySelector(source[i]), elementSelector(source[i]));
            return d;
        }

        private static IJsonEntity ToJsonV0<TSource, TElement>(List<TSource> source, Func<TSource, string> keySelector, Func<TSource, TElement> elementSelector)
        {
            var d = new JsonEntity();
            foreach (var element in source) d.Add(keySelector(element), elementSelector(element));
            return d;
        }
    }
}
