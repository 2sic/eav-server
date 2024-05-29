using System.Runtime.Caching;
using ToSic.Eav.Apps.Internal.Insights;
using ToSic.Razor.Blade;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsMemoryCache() : InsightsProvider(Link, teaser: "Memory Cache Analysis", helpCategory: "Performance")
{
    public static string Link = "MemoryCache";

    public override string Title => "Insights into Memory Cache";

    private const int ShowMax = 500;

    public override string HtmlBody()
    {
        var msg = """
                  welcome to memory cache insights.
                  <br>
                  Try using ?key=xyz to filter.
                  <a href='?key='>No Filter</a> | <a href='?key=Sxc-CacheService'>CacheService</a> | <a href='?key=2sxc.Lightspeed'>LightSpeed</a> | <a href='?key=2sxc.AssemblyCache'>AssemblyCache</a>
                  | <a href='?key=Eav-AppJsonService'>AppJsonService</a>
                  <br>
                  """;
        msg += Tags.Nl2Br("Some Statistics\n"
                          + "\n"
                          + "\n"
        );

        var filterPrefix = Parameters.TryGetValue("key", out var prefix) ? prefix?.ToString() : null;

        msg += MemoryTable(filterPrefix);


        //msg += Linker.LinkTo(view: Name, label: "Run", appId: AppId.Value, more: "type=run");

        return msg;

    }

    internal string MemoryTable(string filterPrefix)
    {
        var l = Log.Fn<string>();

        var cacheList = MemoryCache.Default
            .OrderBy(pair => pair.Key)
            .ToList();

        if (filterPrefix != null)
            cacheList = cacheList
                .Where(pair => pair.Key.StartsWith(filterPrefix))
                .ToList();

        var cacheItems = cacheList
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        var msg = "";
        if (cacheItems.Count > ShowMax)
            msg += P($"There are {cacheItems.Count} items in the cache, which is a lot. Will only show first {ShowMax}. Use filter to reduce results.");
        try
        {
            msg += "<table id='table'>"
                   + InsightsHtmlTable.HeadFields(
                       "#",
                       "Key",
                       "Type",
                       new SpecialField("Size ca.", tooltip: "Size is estimated, which cannot always be done")
                   )
                   + "<tbody>"
                   + "\n";

            var count = 0;
            foreach (var cacheItem in cacheItems)
            {
                // figure out best key
                var fullKey = cacheItem.Key ?? "";
                var visibleKey = fullKey;
                if (fullKey.Length > 85) 
                    visibleKey = fullKey.Substring(0, 40) + "..." + fullKey.Substring(fullKey.Length - 40);

                // Try to figure out memory size
                var size = TryToFigureOutObjectSize(cacheItem.Value);

                msg += InsightsHtmlTable.RowFields(
                    ++count,
                    Span(visibleKey).Title(fullKey),
                    cacheItem.Value?.GetType().Name ?? "null",
                    SpecialField.Right(size > 0 ? size.ToString() : "?")
                ) + "\n";
            }
            msg += "</tbody>" + "\n";
            msg += InsightsHtmlTable.RowFields(
                Strong($"{cacheItems.Count}")
            );
            msg += "</table>";
            msg += "\n\n";
            //msg += P(
            //    $"Total item in system: {items?.Count} - in types: {totalItems} - numbers {Em("should")} match!");
            msg += InsightsHtmlParts.JsTableSort();
        }
        catch (Exception ex)
        {
            l.Ex(ex);
        }

        return l.ReturnAsOk(msg);
    }

    private int TryToFigureOutObjectSize(object value)
    {
        if (value == null) return 0;

        var type = value.GetType();
        if (type.IsValueType)
            return value switch
            {
                string str => str.Length,
                byte[] bytes => bytes.Length,
                int _ => 4,
                long _ => 8,
                double _ => 8,
                float _ => 4,
                decimal _ => 16,
                bool _ => 1,
                char _ => 2,
                DateTime _ => 8,
                DateTimeOffset _ => 16,
                TimeSpan _ => 8,
                Guid _ => 16,
                short _ => 2,
                byte _ => 1,
                uint _ => 4,
                ulong _ => 8,
                ushort _ => 2,
                sbyte _ => 1,
                char[] chars => chars.Length * 2,
                int[] ints => ints.Length * 4,
                long[] longs => longs.Length * 8,
                double[] doubles => doubles.Length * 8,
                _ => -1
            };
        return -1;
    }
}