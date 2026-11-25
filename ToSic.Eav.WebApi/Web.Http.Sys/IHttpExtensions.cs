namespace ToSic.Eav.Web.Http.Sys;

public static class IHttpExtensions
{
    /// <summary>
    /// QueryString params as KeyValue Pairs.
    /// We don't use a dictionary, because sometimes the same key can occur twice.
    /// </summary>
    /// <returns></returns>
    public static List<KeyValuePair<string, string>> QueryStringKeyValuePairs(this IHttp http)
    {
        var qs = http.QueryStringParams;
        return qs
                   ?.AllKeys
                   .Select(key => new KeyValuePair<string, string>(key!, qs[key]!))
                   .ToList()
               ?? [];
    }

}