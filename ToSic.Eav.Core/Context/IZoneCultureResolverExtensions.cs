using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Context;

// ReSharper disable once InconsistentNaming
[PrivateApi("this is all very internal, and names may still change")]
public static class IZoneCultureResolverExtensions
{
    public static string SafeCurrentCultureCode(this IZoneCultureResolver resolver)
        => (resolver?.CurrentCultureCode ?? ThreadCurrentCultureInfo.Name).ToLowerInvariant();

    public static CultureInfo SafeCultureInfo(string[] dimensions)
    {
        try
        {
            if (dimensions == null || dimensions.Length == 0) return ThreadCurrentCultureInfo;
            var d = dimensions.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(d)) return CultureInfo.GetCultureInfo(d);
        }
        catch { /* ignore */}

        return ThreadCurrentCultureInfo;
    }

    private static CultureInfo ThreadCurrentCultureInfo => CultureInfo.CurrentCulture;

    public static CultureInfo SafeCurrentCultureInfo(this IZoneCultureResolver resolver) =>
        resolver == null 
            ? ThreadCurrentCultureInfo 
            : CultureInfo.GetCultureInfo(resolver.SafeCurrentCultureCode());



    #region Language Priorities list

    public static string[] SafeLanguagePriorityCodes(this IZoneCultureResolver resolver)
    {
        if (resolver == null) return new[] { SafeCurrentCultureCode(null), null };

        var list = (resolver as IZoneCultureResolverProWIP)?.CultureCodesWithFallbacks
                   ?? UnsafeLanguagePriorityCodesWithoutProWIP(resolver);

        if (list == null) return new[] { SafeCurrentCultureCode(null), null };

        ListBuildAddFinalFallback(list);
        return list.ToArray();
    }


    private static List<string> UnsafeLanguagePriorityCodesWithoutProWIP(this IZoneCultureResolver resolver)
    {
        if (resolver == null)
            throw new ArgumentNullException(
                "this method is very internal and should never be called with non-existing resolvers",
                nameof(resolver));

        var priorities = new List<string>();
        // 1. First priority: current culture
        ListBuildAddCodeIfNew(priorities, SafeCurrentCultureCode(resolver));
        // 2. Second priority: Fallback culture
        ListBuildAddCodeIfNew(priorities, resolver.DefaultCultureCode);

        // 9. last priority: will be handled by upstream

        // return list
        return priorities;
    }

    public static void ListBuildAddCodeIfNew(List<string> list, string code)
    {
        if (code == null) return;
        code = code.ToLowerInvariant();
        if (!list.Contains(code)) list.Add(code);
    }

    public static void ListBuildAddFinalFallback(List<string> list)
        => list.Add(null);

    #endregion
}