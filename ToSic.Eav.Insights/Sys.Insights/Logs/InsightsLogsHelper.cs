using System.Text;
using ToSic.Eav.Apps.Assets.Sys;
using ToSic.Eav.Sys.Insights.HtmlHelpers;
using ToSic.Razor.Blade;
using static ToSic.Eav.Sys.Insights.HtmlHelpers.InsightsHtmlBase;
using static ToSic.Eav.Sys.Insights.HtmlHelpers.InsightsHtmlTable;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.Logs;

internal class InsightsLogsHelper(ILogStoreLive logStore)
{
    private InsightsHtmlBase Linker { get; } = new();

    internal string LogHistoryOverview()
    {
        var segments = logStore.SegmentCounts();
        var count = 0;
        return P($"Logs Overview: {segments.Count}\n")
               + Table().Id("table").Wrap(
                   HeadFields(["# ↕", "Key ↕", "Count ↕", "Actions ↕"]),
                   Tbody(segments.OrderBy(p => p.Key)
                       .Select(pair => RowFields([
                           (++count).ToString(),
                           Linker.LinkTo(pair.Key, InsightsLogs.Link, key: pair.Key),
                           pair.Value.ToString(),
                           Linker.LinkTo("flush", InsightsLogsFlush.Link, key: pair.Key),
                       ]))
                       .Cast<object>()
                       .ToArray()))
               + "\n\n" + InsightsHtmlParts.JsTableSort();
    }

    internal IHtmlTag? ShowSpecs(LogSnapshot? snapshot)
    {
        if (snapshot == null)
            return null;
        var specs = new Dictionary<string, string>(snapshot.Specs, StringComparer.InvariantCultureIgnoreCase)
        {
            ["Z Timespan A-Start"] = snapshot.Created.Dump(),
            ["Z Store A-LogId"] = snapshot.LogId,
            ["Z Store B-Dropped Entries"] = snapshot.DroppedEntries.ToString(),
            ["Z Store C-Truncated Entries"] = snapshot.TruncatedEntries.ToString(),
        };
        if (snapshot.Entries.Length > 0)
        {
            var first = snapshot.Entries[0].Created;
            var last = snapshot.Entries[snapshot.Entries.Length - 1].Created;
            specs["Z Timespan B-First"] = first.Dump();
            specs["Z Timespan C-Last"] = last.Dump();
            specs["Z Timespan D-Duration SL"] = (last - snapshot.Created).ToString();
            specs["Z Timespan D-Duration FL"] = (last - first).ToString();
        }
        var table = Table(HeadFields([SpecialField.Left("Aspect ↕"), SpecialField.Left("Value ↕")]));
        table = specs.OrderBy(s => s.Key).Aggregate(table,
            (current, spec) => current.Add(Tr(Td(spec.Key), Td(spec.Value))));
        return Div(H2("Log Specs"), table);
    }

    internal string LogHeader(string key, bool showFlush, bool showReset = false)
    {
        var result = +Div("back to " + Linker.LinkTo("2sxc insights home", InsightsHelp.Link))
                     + H1($"2sxc Insights: Log {key}")
                     + P("Status: ", Strong(logStore.Pause ? "paused" : "collecting"),
                         ", toggle: ", Linker.LinkTo(HtmlEncode("▶"), InsightsPauseLogs.Link, more: "toggle=false"),
                         " | ", Linker.LinkTo(HtmlEncode("⏸"), InsightsPauseLogs.Link, more: "toggle=true"),
                         $" collecting #{logStore.AddCount} of max {logStore.MaxItems} (keep max {logStore.SegmentSize} per set, then FIFO); {logStore.Mode}; {logStore.Status}"
                         + (showFlush ? " " + Linker.LinkTo("flush " + key, InsightsLogsFlush.Link, key: key) : ""));
        if (showReset)
            result += Br() + Strong("This list has filters applied. ")
                           + Linker.LinkTo(HtmlEncode("❌") + "remove filters", InsightsLogs.Link, key: key);
        return result.ToString();
    }

    internal string LogHistoryList(string key, string filter)
    {
        var set = logStore.Snapshot(key);
        if (set.Count == 0)
            return "item not found";
        bool HasKey(string name) => set.Any(s => s.Specs.ContainsKey(name));
        string GetVal(IReadOnlyDictionary<string, string> specs, string name)
            => specs.TryGetValue(name, out var value) ? value : "";
        IEnumerable<LogSnapshot> items = set;
        if (filter.HasValue())
            foreach (var part in filter.Split(','))
            {
                var criteria = part.Split('=');
                if (criteria.Length == 2)
                    items = items.Where(item => item.Specs.TryGetValue(criteria[0], out var value)
                                                 && value.EqualsInsensitive(criteria[1]));
            }
        var materialized = items.ToArray();
        var hasApp = HasKey(nameof(IAppIdentity.AppId));
        var hasSite = HasKey("SiteId");
        var hasPage = HasKey("PageId");
        var hasModule = HasKey("ModuleId");
        var hasUser = HasKey("UserId");
        var totalBytes = materialized.Sum(s => s.EstimatedBytes);
        var result = P($"Logs Overview: {set.Count}\n")
                     + Table().Id("table").Wrap(
                         HeadFields(["#", "Timestamp UTC", hasApp ? "App ↕" : null, hasSite ? "Site ↕" : null,
                             hasPage ? "Page ↕" : null, hasModule ? "Mod ↕" : null, hasUser ? "Usr ↕" : null,
                             SpecialField.Right("Lines"), SpecialField.Right("Size ca."),
                             SpecialField.Left("Title / First Message"), "Info", "Time"]),
                         Tbody(materialized.Select((snapshot, index) =>
                         {
                             var first = snapshot.Entries.FirstOrDefault();
                             var title = (snapshot.Title ?? first?.Message).NeverNull();
                             var trimmed = title.Length <= 150 ? title : title.Substring(0, 150) + "…";
                             return RowFields([
                                 (index + 1).ToString(),
                                 Linker.LinkTo(snapshot.Created.ToUniversalTime().ToString("O").Substring(5), InsightsLogs.Link,
                                     key: key, more: $"position={index + 1}"),
                                 !hasApp ? null : SpecialField.Right(GetVal(snapshot.Specs, nameof(IAppIdentity.AppId)), tooltip: GetVal(snapshot.Specs, "AppName")),
                                 !hasSite ? null : SpecialField.Right(GetVal(snapshot.Specs, "SiteId")),
                                 !hasPage ? null : SpecialField.Right(GetVal(snapshot.Specs, "PageId")),
                                 !hasModule ? null : SpecialField.Right(GetVal(snapshot.Specs, "ModuleId")),
                                 !hasUser ? null : SpecialField.Right(GetVal(snapshot.Specs, "UserId")),
                                 SpecialField.Right($"{snapshot.Entries.Length:##,###}"),
                                 SpecialField.Right($"{new SizeInfo(snapshot.EstimatedBytes).Kb:N} KB"),
                                 SpecialField.Left(HtmlEncode(trimmed), tooltip: title), HtmlEncode(first?.Result),
                                 SpecialField.Right(new InsightsTime().ShowTime(snapshot)),
                             ]);
                         }).ToArray<object>()));
        return result + "\n\n" + Br() + Strong($"Total Log Size in Memory: {new SizeInfo(totalBytes).Mb:N} MB")
               + Br() + InsightsHtmlParts.JsTableSort();
    }

    internal string DumpTree(string title, ILog? log)
        => DumpTree(title, logStore.Snapshot(log));

    internal string DumpTree(string title, LogSnapshot? snapshot)
    {
        if (snapshot == null)
            return P("log snapshot is unavailable").ToString();
        if (snapshot.Entries.Length == 0)
            return "";
        _lastLogLabel = null;
        var html = new StringBuilder(H1(title) + Div(snapshot.Created.Dump()) + "\n\n<ol>\n");
        var emitted = new HashSet<long>();
        AppendChildren(html, snapshot, null, "", default, emitted);
        // Keep malformed or interrupted calls visible instead of silently losing them.
        foreach (var orphan in snapshot.Entries.Where(e => !emitted.Contains(e.Sequence)))
            AppendOne(html, snapshot, orphan, "", default, emitted);
        html.Append("</ol>end of log");
        return html.ToString();
    }

    private void AppendChildren(StringBuilder html, LogSnapshot snapshot, long? parentOperation,
        string breadcrumb, TimeSpan parentTime, HashSet<long> emitted)
    {
        foreach (var entry in snapshot.Entries.Where(e =>
                     (e.WrapOpen ? e.ParentOperationId : e.OperationId) == parentOperation))
            AppendOne(html, snapshot, entry, breadcrumb, parentTime, emitted);
    }

    private void AppendOne(StringBuilder html, LogSnapshot snapshot, LogEvent entry,
        string breadcrumb, TimeSpan parentTime, HashSet<long> emitted)
    {
        if (!emitted.Add(entry.Sequence))
            return;
        html.AppendLine("<li>");
        html.AppendLine(TreeDumpOneLine(entry, breadcrumb, parentTime,
            new InsightsTime(FullTimespan(snapshot)), snapshot.Created));
        if (entry.WrapOpen)
        {
            if (!entry.WrapOpenWasClosed)
                html.AppendLine(HtmlEncode("🪵⚠️ LOGGER WARNING: This logger was never closed"));
            html.AppendLine("<ol>");
            AppendChildren(html, snapshot, entry.Sequence, entry.ShortSource, entry.Elapsed, emitted);
            html.AppendLine("</ol>");
        }
        html.AppendLine("</li>");
    }

    private static TimeSpan FullTimespan(LogSnapshot snapshot)
    {
        var last = snapshot.Entries.Last();
        return last.Created.Add(last.IsTimed ? last.Elapsed : default) - snapshot.Created;
    }

    private static string KeepOnlyLastSegmentOfPath(string label)
        => label.Count(c => c == '[') > 1
            ? label.Substring(0, label.Length - 2).AfterLast("]") + "]"
            : label;

    private string TreeDumpOneLine(LogEvent entry, string parentBreadcrumb, TimeSpan parentTime,
        InsightsTime time, DateTime mainStart)
    {
        var path = entry.Source;
        var label = KeepOnlyLastSegmentOfPath(path);
        if (parentBreadcrumb.HasValue() && path.HasValue())
        {
            var previous = path.IndexOf(parentBreadcrumb, StringComparison.Ordinal);
            if (previous > 0)
            {
                var cut = previous + parentBreadcrumb.Length;
                if (label.Length <= cut)
                    cut = previous;
                var rest = path.Substring(cut);
                label = KeepOnlyLastSegmentOfPath(rest);
                if (rest != label)
                    label = '…' + label;
            }
        }
        if (label.Trim('…') == _lastLogLabel)
            label = "[=]";
        else
            _lastLogLabel = label;
        var message = HtmlEncode(entry.Message.NeverNull());
        if (entry.ShowNewLines)
            message = Tags.Nl2Br(message).Replace("<br><br>", "<br>");
        return Span(HoverLabel(HtmlEncode(label), path, "logIds") + " - " + message
                    + (entry.Result != null ? $" {ResStart}{HtmlEncode(entry.Result)}{ResEnd}" : "")
                    + time.ShowTime(entry, parentTime, mainStart)
                    + (entry.Code != null && !entry.HideCodeReference
                        ? " " + HoverLabel("C#", $"{entry.Code.Path} - {entry.Code.Name}() #{entry.Code.Line}", "codePeek")
                        : "") + "\n").Class("log-line").ToString();
    }

    private const string ResStart = "<span style='color: green'>= ";
    private const string ResEnd = "</span>";
    private string? _lastLogLabel;
}
