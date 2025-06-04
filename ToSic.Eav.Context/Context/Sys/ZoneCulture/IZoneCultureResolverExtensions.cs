using System.Globalization;

namespace ToSic.Eav.Context.Sys.ZoneCulture;

// ReSharper disable once InconsistentNaming
[PrivateApi("this is all very internal, and names may still change")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public static class IZoneCultureResolverExtensions
{
    /// <summary>
    /// Get the lower-cased current culture code, or the thread's current culture if the resolver is null
    /// </summary>
    /// <param name="resolver"></param>
    /// <returns></returns>
    public static string SafeCurrentCultureCode(this IZoneCultureResolver resolver)
        => (resolver?.CurrentCultureCode ?? CultureInfo.CurrentCulture.Name).ToLowerInvariant();

    public static CultureInfo SafeCurrentCultureInfo(this IZoneCultureResolver resolver)
        => resolver == null 
            ? CultureInfo.CurrentCulture
            : CultureInfo.GetCultureInfo(resolver.SafeCurrentCultureCode());



    #region Language Priorities list

    /// <summary>
    /// Complete list of culture codes with fallbacks, lower-cased guaranteed.
    /// Includes trailing `null` for auto-null fallback.
    /// </summary>
    /// <param name="resolver"></param>
    /// <returns></returns>
    public static string[] SafeLanguagePriorityCodes(this IZoneCultureResolver resolver)
    {
        // no resolver - return current culture; safe, lower-case, with auto-null fallback
        if (resolver == null)
            return [SafeCurrentCultureCode(null), null];

        // If priorities are possible, return them - lower case guaranteed
        var list = (resolver as IZoneCultureResolverProWIP)?.CultureCodesWithFallbacks
                   ?? UnsafeLanguagePriorityCodesWithoutPrioWIP(resolver);

        // if list is null, return current culture; safe, lower-case, with auto-null fallback
        if (list == null)
            return [SafeCurrentCultureCode(null), null];

        // Finalize, with trailing null.
        return [.. list, null];
    }

    /// <summary>
    /// Get a list of culture codes with fallback by primary, then default. 
    /// </summary>
    /// <param name="resolver"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static List<string> UnsafeLanguagePriorityCodesWithoutPrioWIP(this IZoneCultureResolver resolver)
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

    /// <summary>
    /// Add the code if it's not in the list yet, - lowercased guaranteed
    /// </summary>
    /// <param name="list"></param>
    /// <param name="code"></param>
    public static void ListBuildAddCodeIfNew(List<string> list, string code)
    {
        if (code == null) return;
        code = code.ToLowerInvariant();
        if (!list.Contains(code)) list.Add(code);
    }

    #endregion
}