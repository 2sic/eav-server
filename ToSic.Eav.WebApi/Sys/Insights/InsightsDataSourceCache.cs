﻿using System.Linq;
using System.Text;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.Plumbing;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using ToSic.Razor.Blade;
using static ToSic.Eav.DataSource.Internal.Caching.DataSourceListCache;
using static ToSic.Eav.WebApi.Sys.Insights.InsightsHtmlBase;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class InsightsDataSourceCache: ServiceBase
{
    private readonly LazySvc<IDataSourceCacheService> _dsCacheSvc;
    public InsightsDataSourceCache(LazySvc<IDataSourceCacheService> dsCacheSvc): base("Ins.DsCache")
    {
        ConnectServices(
            _dsCacheSvc = dsCacheSvc
        );
    }

    internal InsightsHtmlBase Html = new();

    public string DataSourceCache()
    {
        var locks = LoadLocks;
        var m = new StringBuilder();
        m.AppendLine(H1($"DataSource Lists Cache ({locks.Count})").ToString());


        var namesInMemory = locks.Select(l => l.Key).Where(HasStream).ToList();
        m.AppendLine(H1(
                $"In Cache ({namesInMemory.Count})",
                Html.LinkTo(HtmlEncode("🚽"), nameof(DataSourceCacheFlushAll))
            )
            .ToString());

        m.AppendLine("<table id='table'>"
                     + InsightsHtmlTable.HeadFields("Name ↕", "Count ↕", "Timestamp", "AutoRefresh", "Flush")
                     + "<tbody>");

        var rows = namesInMemory.Select(name =>
        {
            var cacheItem = GetStream(name);
            return InsightsHtmlTable.RowFields(
                HoverLabel(name.Ellipsis(100), name.Replace(">", "\n>"), ""),
                SpecialField.Right(
                    cacheItem?.List.Count
                ),
                cacheItem?.CacheTimestamp.ToString(),
                SpecialField.Center(
                    EmojiTrueFalse(cacheItem?.RefreshOnSourceRefresh ?? false)
                ),
                Html.LinkTo(HtmlEncode("🚽"), nameof(DataSourceCacheFlush), key: name)

            );
        });

        foreach (var r in rows) m.AppendLine(r.ToString());

        m.AppendLine("</tbody>"
                     + "</table>");

        m.AppendLine(Hr().ToString());
        var namesOutOfMemory = locks.Select(l => l.Key).Where(n => !HasStream(n)).ToList();
        m.AppendLine(H2($"Not In Cache ({namesOutOfMemory.Count})").ToString());

        if (!namesOutOfMemory.Any())
            m.AppendLine("(none)");
        else
        {
            m.AppendLine("<ol>");
            foreach (var name in namesOutOfMemory)
                m.AppendLine(Li(name).ToString());
            m.AppendLine("</ol>");
        }


        m.AppendLine(InsightsHtmlParts.JsTableSort().ToString());

        return m.ToString();
    }

    public string DataSourceCacheItem(string key)
    {
        var m = new StringBuilder();
        m.AppendLine(H1("App In Cache").ToString());

        var locks = LoadLocks;

        var namesInMemory = locks.Select(l => l.Key).Where(key.EqualsInsensitive).Where(HasStream).ToList();

        var name = namesInMemory.FirstOrDefault();
        var cacheItem = GetStream(name);

        m.AppendLine("<table id='table'>"
                     + InsightsHtmlTable.HeadFields("Name", "Count", "Timestamp")
                     + "<tbody>");

        var row = InsightsHtmlTable.RowFields(
            name,
            cacheItem?.List.Count,
            cacheItem?.CacheTimestamp.ToString()
        );

        m.AppendLine(row.ToString());

        m.AppendLine("</tbody>"
                     + "</table>");
        m.AppendLine(InsightsHtmlParts.JsTableSort().ToString());

        return m.ToString();
    }

    public string DataSourceCacheFlushAll()
    {
        _dsCacheSvc.Value.FlushAll();
        return "All Flushed" + Br() + Html.LinkBack();
    }

    public string DataSourceCacheFlush(string key)
    {
        _dsCacheSvc.Value.Flush(key);
        return $"key {key} Flushed" + Br() + Html.LinkBack();
    }
}