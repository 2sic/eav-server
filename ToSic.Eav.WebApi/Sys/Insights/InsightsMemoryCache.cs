﻿using System.Runtime.Caching;
using ToSic.Eav.Apps.Internal.Insights;
using ToSic.Razor.Blade;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsMemoryCache() : InsightsProvider(Link, teaser: "Memory Cache Analysis", helpCategory: "Performance")
{
    public static string Link = "MemoryCache";

    public override string Title => "Insights into Memory Cache";

    private const int ShowMax = 500;

    private MemorySizeEstimator SizeEstimator => _sizeEstimator ??= new(Log);
    private MemorySizeEstimator _sizeEstimator;

    public override string HtmlBody()
    {
        var msg = """
                  welcome to memory cache insights.
                  <br>
                  Try using ?key=xyz to filter.
                  <a href='?key='>No Filter</a> | <a href='?key=Sxc-CacheService'>CacheService</a> | <a href='?key=Sxc-LightSpeed'>LightSpeed</a> | <a href='?key=Sxc-AssemblyCache'>AssemblyCache</a>
                  | <a href='?key=Eav-AppJsonService'>AppJsonService</a>
                  <br>
                  """;
        msg += Tags.Nl2Br("Some Statistics\n"
                          + "\n"
                          + "\n"
        );

        var filterPrefix = Parameters.TryGetValue("key", out var prefix) ? prefix?.ToString() : null;
        var filterType = Parameters.TryGetValue("type", out var type) ? type?.ToString() : null;

        msg += MemoryTable(filterPrefix, filterType);


        //msg += Linker.LinkTo(view: Name, label: "Run", appId: AppId.Value, more: "type=run");

        return msg;

    }

    internal string MemoryTable(string filterPrefix, string filterType)
    {
        var l = Log.Fn<string>();

        var all = MemoryCache.Default
            .OrderBy(pair => pair.Key)
            .Select(pair => new
            {
                Key = pair.Key,
                Value = pair.Value,
                TypeName = (pair.Value?.GetType().Name ?? "unknown"),
            })
            .ToList();

        var filtered = all;

        if (filterPrefix != null)
            filtered = filtered.Where(pair => pair.Key.StartsWith(filterPrefix)).ToList();

        if (filterType != null)
            filtered = filtered.Where(pair => pair.TypeName == filterType).ToList();



        //var cacheItems = all
        //    .ToDictionary(pair => pair.Key, pair => pair.Value);

        var msg = "";
        if (filtered.Count > ShowMax)
            msg += P($"There are {filtered.Count} items in the cache, which is a lot. Will only show first {ShowMax}. Use filter to reduce results.");
        try
        {
            msg += "<table id='table'>"
                   + InsightsHtmlTable.HeadFields(
                       "#",
                       SpecialField.Left("Key ↕"),
                       SpecialField.Left(
                           InsightsHtmlBase.HtmlEncode("Type ↕")
                           + filterType.NullOrGet(() => Linker.LinkTo(view: Link, label: InsightsHtmlBase.HtmlEncode("✖️"), type: "")),
                           isEncoded: true
                       ),
                       SpecialField.Left("Size ca. ↕", tooltip: "Size is estimated, which cannot always be done"),
                       SpecialField.Center("Quality", tooltip: "Size estimate reliability")
                   )
                   + "<tbody>"
                   + "\n";

            var count = 0;
            var totalSize = new SizeEstimate();
            foreach (var cacheItem in filtered)
            {
                // figure out best key
                var fullKey = cacheItem.Key ?? "";
                var visibleKey = fullKey;
                if (fullKey.Length > 85) 
                    visibleKey = fullKey.Substring(0, 40) + "..." + fullKey.Substring(fullKey.Length - 40);

                // Try to figure out memory size
                var estimate = SizeEstimator.Estimate(cacheItem.Value);
                totalSize += estimate;
                //var size = estimate.Known + estimate.Estimated;
                //var icon = estimate.Error
                //    ? "⚠️"
                //    : estimate.Unknown || estimate.Known == 0
                //        ? "❔"
                //        : "✅";

                var typeName = cacheItem.Value?.GetType().Name ?? "null";

                msg += InsightsHtmlTable.RowFields(
                    ++count,
                    Span(visibleKey).Title(fullKey),
                    Linker.LinkTo(view: Link, label: typeName, type: typeName),
                    SpecialField.Right(estimate.Total > 0
                        ? estimate.Total.ToString()
                        : "-"
                    ),
                    InsightsHtmlBase.HtmlEncode(estimate.Icon)
                ) + "\n";
            }
            msg += "</tbody>" + "\n";

            // Footer
            var typeCountAll = all
                .GroupBy(pair => pair.TypeName)
                .Count();

            var typeCountFiltered = filtered
                .GroupBy(pair => pair.TypeName)
                .Count();

            msg += InsightsHtmlTable.RowFields(
                Strong($"{filtered.Count} of {all.Count}"),
                "",
                $"{typeCountFiltered} of {typeCountAll}",
                SpecialField.Right(totalSize.Total > 0
                                        ? totalSize.Total.ToString()
                                        : "-"
                                    ),
                InsightsHtmlBase.HtmlEncode(totalSize.Icon)
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

    //private int TryToFigureOutObjectSize(object value)
    //{
    //    if (value == null) return 0;

    //    var type = value.GetType();
    //    if (type.IsValueType)
    //        return value switch
    //        {
    //            string str => str.Length,
    //            byte[] bytes => bytes.Length,
    //            int _ => 4,
    //            long _ => 8,
    //            double _ => 8,
    //            float _ => 4,
    //            decimal _ => 16,
    //            bool _ => 1,
    //            char _ => 2,
    //            DateTime _ => 8,
    //            DateTimeOffset _ => 16,
    //            TimeSpan _ => 8,
    //            Guid _ => 16,
    //            short _ => 2,
    //            byte _ => 1,
    //            uint _ => 4,
    //            ulong _ => 8,
    //            ushort _ => 2,
    //            sbyte _ => 1,
    //            char[] chars => chars.Length * 2,
    //            int[] ints => ints.Length * 4,
    //            long[] longs => longs.Length * 8,
    //            double[] doubles => doubles.Length * 8,
    //            _ => -1
    //        };
    //    return -1;
    //}
}