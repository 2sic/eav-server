using System.Text;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.Security.Encryption;
using ToSic.Razor.Blade;
using static ToSic.Eav.WebApi.Sys.Insights.InsightsHtmlBase;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class InsightsDataSourceCache(
    LazySvc<IDataSourceCacheService> dsCacheSvc,
    IListCacheSvc listCacheSvc)
    : ServiceBase("Ins.DsCache", connect: [dsCacheSvc, listCacheSvc])
{
    internal InsightsHtmlBase Html = new();

    public string DataSourceCache()
    {
        var locks = listCacheSvc.LoadLocks.Locks;
        var m = new StringBuilder();
        m.AppendLine(H1($"DataSource Lists Cache ({locks.Count})").ToString());


        var namesInMemory = locks.Select(l => l.Key)
            .Where(listCacheSvc.HasStream)
            .ToList();

        m.AppendLine(H1(
                $"In Cache ({namesInMemory.Count})",
                Html.LinkTo(HtmlEncode("🚽 flush all"), nameof(DataSourceCacheFlushAll))
            )
            .ToString());

        m.AppendLine("<table id='table'>"
                     + InsightsHtmlTable.HeadFields("Name ↕", "Count ↕", "Timestamp", "AutoRefresh", "Flush")
                     + "<tbody>");

        var rows = namesInMemory.Select(name =>
        {
            var cacheItem = listCacheSvc.Get(name);
            var keyHash = Sha256.Hash(name);
            return InsightsHtmlTable.RowFields(
                HoverLabel(name.Ellipsis(100), name.Replace(">", "\n>"), ""),
                SpecialField.Right(
                    cacheItem?.List.Count
                ),
                cacheItem?.CacheTimestamp.ToString(),
                SpecialField.Center(
                    EmojiTrueFalse(cacheItem?.RefreshOnSourceRefresh ?? false)
                ),
                Html.LinkTo(HtmlEncode("🚽"), nameof(DataSourceCacheFlush), key: keyHash)
            );
        });

        foreach (var r in rows) m.AppendLine(r.ToString());

        m.AppendLine("</tbody>"
                     + "</table>");

        m.AppendLine(Hr().ToString());
        var namesOutOfMemory = locks.Select(l => l.Key)
            .Where(n => !listCacheSvc.HasStream(n))
            .ToList();

        m.AppendLine(H2($"Not in Cache any More ({namesOutOfMemory.Count})").ToString());

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

        var locks = listCacheSvc.LoadLocks.Locks;

        var namesInMemory = locks.Select(l => l.Key)
            .Where(key.EqualsInsensitive)
            .Where(listCacheSvc.HasStream)
            .ToList();

        var name = namesInMemory.FirstOrDefault();
        var cacheItem = listCacheSvc.Get(name);

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
        dsCacheSvc.Value.FlushAll();
        return "All Flushed" + Br() + Html.LinkBack();
    }

    public string DataSourceCacheFlush(string key)
    {
        // find entry with this key, since the key is a Sha256 hash of the name
        var locks = listCacheSvc.LoadLocks.Locks;
        var name = locks.Select(l => l.Key)
            .FirstOrDefault(k => Sha256.Hash(k) == key);

        if (name == null)
            return $"key {key} not found" + Br() + Html.LinkBack();

        dsCacheSvc.Value.Flush(name);
        return $"Key flushed: '{key}'. {Br()}Name was: {Pre(Tags.Nl2Br(name))}" + Br() + Html.LinkBack();
    }
}