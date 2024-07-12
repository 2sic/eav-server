using System.Text;
using ToSic.Eav.Apps.Assets.Internal;
using ToSic.Lib.Memory;
using ToSic.Razor.Blade;
using static ToSic.Eav.WebApi.Sys.Insights.InsightsHtmlBase;
using static ToSic.Eav.WebApi.Sys.Insights.InsightsHtmlTable;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsLogsHelper(ILogStoreLive logStore)
{
    private InsightsHtmlBase Linker { get; } = new();

    internal string LogHistoryOverview()
    {
        var msg = "";
        try
        {
            var segments = logStore.Segments;
            msg += P($"Logs Overview: {segments.Count}\n");

            var count = 0;

            msg += Table().Id("table").Wrap(
                HeadFields("# ↕", "Key ↕", "Count ↕", "Actions ↕"),
                Tbody(
                    segments.OrderBy(segPair => segPair.Key)
                        .Select(segPair => RowFields((++count).ToString(),
                            Linker.LinkTo(segPair.Key, InsightsLogs.Link, key: segPair.Key),
                            $"{segPair.Value.Count}",
                            Linker.LinkTo("flush", InsightsLogsFlush.Link, key: segPair.Key))
                        )
                        .Cast<object>()
                        .ToArray())
            );
            msg += "\n\n";
            msg += InsightsHtmlParts.JsTableSort();
        }
        catch
        {
            // ignored
        }
        return msg;
    }


    internal IHtmlTag ShowSpecs(LogStoreEntry entry)
    {
        if (entry == null) return null;

        var specs = entry.Specs ?? new Dictionary<string, string>();
        // if (specs == null) return null;

        var specList = Table(HeadFields(SpecialField.Left("Aspect ↕"), SpecialField.Left("Value ↕")));

        var specsCopy = new Dictionary<string, string>(specs, StringComparer.InvariantCultureIgnoreCase);
        if (entry.Log is Log log && log.Entries.Count > 0)
        {
            specsCopy["Z Timespan A-Start"] = log.Created.Dump();

            var first = log.Entries.First()?.Created;
            specsCopy["Z Timespan B-First"] = first?.Dump();
            var last = log.Entries.Last()?.Created;
            specsCopy["Z Timespan C-Last"] = last?.Dump();
            if (last != null)
                specsCopy["Z Timespan D-Duration SL"] = (last - log.Created).ToString();
            if (first != null && last != null)
                specsCopy["Z Timespan D-Duration FL"] = (last - first).ToString();
        }

        specList = specsCopy
            .OrderBy(s => s.Key)
            .Aggregate(specList, (current, spec) 
                => current.Add(Tr(Td(spec.Key), Td(spec.Value))));

        return Div(H2("Log Specs"), specList);
    }

    internal string LogHeader(string key, bool showFlush, bool showReset = false)
    {
        var msg =
            +Div("back to " + Linker.LinkTo("2sxc insights home", InsightsHelp.Link))
            + H1($"2sxc Insights: Log {key}")
            + P("Status: ",
                Strong(logStore.Pause ? "paused" : "collecting"),
                ", toggle: ",
                Linker.LinkTo(HtmlEncode("▶"), InsightsPauseLogs.Link, more: "toggle=false"),
                " | ",
                Linker.LinkTo(HtmlEncode("⏸"), InsightsPauseLogs.Link, more: "toggle=true"),
                $" collecting #{logStore.AddCount} of max {logStore.MaxItems} (keep max {logStore.SegmentSize} per set, then FIFO)"
                + (showFlush
                    ? " " + Linker.LinkTo("flush " + key, InsightsLogsFlush.Link, key: key).ToString()
                    : "")
            );
        if (showReset)
            msg += Br()
                   + Strong("This list has filters applied. ")
                   + Linker.LinkTo(HtmlEncode("❌") + "remove filters", InsightsLogs.Link, key: key);
                
        return msg.ToString();
    }


    internal string LogHistoryList(string key, string filter)
    {
        var msg = "";
        if (!logStore.Segments.TryGetValue(key, out var set))
            return msg + "item not found";

        // Helper to check if any log has this key
        bool HasKey(string k) => set.Any(s => s.Specs?.ContainsKey(k) == true);

        // Helper to get the correct value depending of if it should fill the column, or it's found...
        string GetValOrAlt(bool use, IDictionary<string, string> specs, string k) => !use 
            ? null 
            : specs?.TryGetValue(k, out var value) == true
                ? value
                : "";

        var logItems = set as IEnumerable<LogStoreEntry>;
        if (filter.HasValue())
        {
            var parts = filter.Split(',');
            foreach (var part in parts)
            {
                var criteria = part.Split('=');
                if (criteria.Length != 2) continue;
                logItems = logItems.Where(l =>
                    l.Specs != null && l.Specs.TryGetValue(criteria[0], out var val) &&
                    val.EqualsInsensitive(criteria[1]));
            }
        }
                

        var hasApp = HasKey(nameof(IAppIdentity.AppId));
        var hasSite = HasKey("SiteId");
        var hasPage = HasKey("PageId");
        var hasMod = HasKey("ModuleId");
        var hasUsr = HasKey("UserId");
        var totalSize = new SizeEstimate();
        msg += P($"Logs Overview: {set.Count}\n");
        msg += Table().Id("table").Wrap(
            HeadFields(
                "#",
                "Timestamp",
                hasApp ? "App ↕" : null,
                hasSite ? "Site ↕" : null,
                hasPage ? "Page ↕" : null,
                hasMod ? "Mod ↕" : null,
                hasUsr ? "Usr ↕" : null,
                SpecialField.Right("Lines"),
                SpecialField.Right("Size ca.", tooltip: "Estimated size of this log in memory"),
                SpecialField.Left("Title / First Message"),
                "Info",
                "Time"
            ),
            Tbody(logItems
                .Select((bundle, i) =>
                {
                    var realLog = bundle.Log as Log;
                    var firstIfExists = realLog?.Entries.FirstOrDefault();
                    var specs = bundle.Specs;
                    var timestamp = realLog?.Created.ToUniversalTime().ToString("O").Substring(5) ?? "no timestamp";
                    var size = realLog?.EstimateSize(null);
                    var sizeInfo = size == null ? null : new SizeInfo(size.Total);
                    if (size != null)
                        totalSize += size;

                    var bestTitle = (bundle.Title ?? firstIfExists?.Message).NeverNull();

                    return RowFields(
                        $"{i + 1}",
                        Linker.LinkTo(timestamp, InsightsLogs.Link, key: key, more: $"position={i + 1}"),
                        SpecialField.Right(GetValOrAlt(hasApp, specs, nameof(IAppIdentity.AppId)), tooltip: GetValOrAlt(hasApp, specs, "AppName") ),
                        SpecialField.Right(GetValOrAlt(hasSite, specs, "SiteId")),
                        SpecialField.Right(GetValOrAlt(hasPage, specs, "PageId")),
                        SpecialField.Right(GetValOrAlt(hasMod, specs, "ModuleId")),
                        SpecialField.Right(GetValOrAlt(hasUsr, specs, "UserId")),
                        SpecialField.Right($"{realLog?.Entries.Count:##,###}"),
                        SpecialField.Right(sizeInfo != null ? $"{sizeInfo.Kb:N} KB" : "-"),
                        SpecialField.Left(HtmlEncode(bestTitle.Ellipsis(150, "…")), tooltip: bestTitle),
                        HtmlEncode(firstIfExists?.Result),
                        SpecialField.Right(new InsightsTime().ShowTime(realLog))
                    );
                })
                .ToArray<object>()));
        msg += "\n\n";
        var totalSizeInfo = new SizeInfo(totalSize.Total);
        msg += Br() + Strong($"Total Log Size in Memory: {totalSizeInfo.Mb:N} MB") + Br();
        msg += InsightsHtmlParts.JsTableSort();

        return msg;
    }

    internal string DumpTree(string title, ILog log)
    {
        var typedLog = (Log)log;
        var lg = new StringBuilder(
            H1($"{title}") +
            Div($"{typedLog.Created.Dump()}") +
            "\n\n"
        );

        if (typedLog is { Entries.Count: 0 })
            return "";

        lg.AppendLine("<ol>");

        var breadcrumb = new Stack<string>();
        var times = new Stack<TimeSpan>();

        var entries = typedLog.Entries;
        //var firstEntry = entries.FirstOrDefault();
        //var lastEntry = entries.LastOrDefault();

        //var lastTime = (lastEntry?.Created ?? default) + typedLog.Created;
        //var fullTimeSpan = firstEntry == null
        //    ? default
        //    : firstEntry.Elapsed != default
        //        ? firstEntry.Elapsed
        //        : lastEntry == null
        //            ? default
        //            : lastEntry.Created - typedLog.Created;// firstEntry.Created;
        var ts = FullTimespan(typedLog);
        var maxTimeSpan = new InsightsTime(ts); //fullTimeSpan);
        var startCreated = typedLog.Created;

        foreach (var e in entries)
        {
            // a wrap-close should happen before adding a line, since it must go up a level
            if (e.WrapClose)
            {
                // Go up one level
                lg.AppendLine("</ol></li>");
                if (breadcrumb.Count > 0) breadcrumb.Pop();
                if (times.Count > 0) times.Pop();
            }
            else
            {
                // Create an entry
                lg.AppendLine("<li>");
                var prevBreadcrumb = breadcrumb.Count > 0 ? breadcrumb.Peek() : "";
                lg.AppendLine(TreeDumpOneLine(e, prevBreadcrumb, times.Count > 0 ? times.Peek() : default, maxTimeSpan, startCreated));
                // 
                if (e.WrapOpen)
                {
                    // Go down one level
                    if (!e.WrapOpenWasClosed) lg.AppendLine("LOGGER WARNING: This logger was never closed");
                    breadcrumb.Push(e.ShortSource);
                    times.Push(e.Elapsed);
                    lg.AppendLine("<ol>");
                }
                else lg.AppendLine("</li>");
            }
        }

        lg.Append("</ol>");
        lg.Append("end of log");
        return lg.ToString();
    }

    private static TimeSpan FullTimespan(Log log)
    {
        if (log.Entries.Count == 0) return default;
        var startTime = log.Created;

        var last = log.Entries.Last();
        var endTime = last.Created.Add(last.Elapsed);

        var timeSpan = endTime - startTime;
        return timeSpan;
    }

    /// <summary>
    /// If it has more than one segment, shorten it to last
    /// eg. Eav.Xyz[A7]Dyn.DnnCdr[62]Sxc.RzrHlp[93] => Sxc.RzrHlp[93]
    /// </summary>
    /// <param name="label"></param>
    /// <returns></returns>
    private static string KeepOnlyLastSegmentOfPath(string label) 
        => label.Count(c => c == '[') > 1 
            ? label.Substring(0, label.Length - 2).AfterLast("]") + "]" 
            : label;

    private string TreeDumpOneLine(Entry e, string parentBreadcrumb, TimeSpan parentTime, InsightsTime time, DateTime mainStart)
    {
        // if it's just a close, only repeat the result
        if (e.WrapClose)
            return $"{ResStart}{e.Result}{ResEnd}";

        #region find perfect Label

        var logChainPath = e.Source ?? "";
        var label = KeepOnlyLastSegmentOfPath(logChainPath);

        // If we have a previous breadcrumb...
        try
        {
            if (parentBreadcrumb.HasValue())
            {
                if (logChainPath.HasValue())
                {
                    var prevIndex = logChainPath.IndexOf(parentBreadcrumb, StringComparison.Ordinal);
                    if (prevIndex > 0)
                    {
                        var cut = prevIndex + parentBreadcrumb.Length;
                        if (label.Length <= cut) cut = prevIndex;
                        var rest = logChainPath.Substring(cut);
                        label = KeepOnlyLastSegmentOfPath(logChainPath.Substring(cut));
                        if (rest != label) label = '…' + label;
                    }
                }
                //else
                //    label = '…' + label;
            }
        }
        catch { /* ignore */ }

        // Shorten display if it's the same as the previous line
        if (label.Trim('…') == _lastLogLabel) label = "[=]";
        else _lastLogLabel = label;

        #endregion

        var message = HtmlEncode(e.Message.NeverNull());
        if (e.Options?.ShowNewLines == true) message = Tags.Nl2Br(message).Replace("<br><br>", "<br>");
        return Span(
                HoverLabel(HtmlEncode(label), logChainPath, "logIds")
                + " - "
                + message
                + (e.Result != null
                    ? $" {ResStart}{HtmlEncode(e.Result)}{ResEnd}"
                    : string.Empty)
                + time.ShowTime(e, parentTime, mainStart)
                + (e.Code != null && e.Options?.HideCodeReference != true
                    ? " " + HoverLabel("C#", $"{e.Code.Path} - {e.Code.Name}() #{e.Code.Line}", "codePeek")
                    : string.Empty)
                + "\n")
            .Class("log-line")
            .ToString();
    }
    private const string ResStart = "<span style='color: green'>= ";
    private const string ResEnd = "</span>";

    private string _lastLogLabel;

}