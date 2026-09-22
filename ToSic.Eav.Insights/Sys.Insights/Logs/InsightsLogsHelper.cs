using System.Text;
using ToSic.Eav.Apps.Assets.Sys;
using ToSic.Eav.Sys.Insights.HtmlHelpers;
using ToSic.Razor.Blade;
using ToSic.Sys.Memory;
using static ToSic.Eav.Sys.Insights.HtmlHelpers.InsightsHtmlBase;
using static ToSic.Eav.Sys.Insights.HtmlHelpers.InsightsHtmlTable;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.Logs;

internal class InsightsLogsHelper(IInsightsLogSnapshotReader logReader)
{
    private InsightsHtmlBase Linker { get; } = new();

    internal string LogHistoryOverview()
    {
        var msg = "";
        try
        {
            var segments = logReader.ListGroups()
                .SelectMany(group => group.Segments)
                .Where(segment => segment != null)
                .GroupBy(segment => segment!, StringComparer.InvariantCultureIgnoreCase)
                .OrderBy(group => group.Key);
            msg += P($"Logs Overview: {segments.Count()}\n");

            var count = 0;

            msg += Table().Id("table").Wrap(
                HeadFields(["# ↕", "Key ↕", "Count ↕", "Actions ↕"]),
                Tbody(
                    segments.Select(segment => RowFields([(++count).ToString(),
                            Linker.LinkTo(segment.Key, InsightsLogs.Link, key: segment.Key),
                            $"{segment.Count()}",
                            Linker.LinkTo("flush", InsightsLogsFlush.Link, key: segment.Key)
                            ])
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


    internal IHtmlTag? ShowSpecs(InsightsLogGroupSnapshot? group)
    {
        if (group == null)
            return null;

        var specs = group.Specs;

        var specList = Table(HeadFields([SpecialField.Left("Aspect ↕"), SpecialField.Left("Value ↕")]));

        var specsCopy = new Dictionary<string, string?>(specs, StringComparer.InvariantCultureIgnoreCase)
        {
            ["Z Timespan A-Start"] = group.TimestampUtc.Dump(),
            ["Z Timespan B-First"] = group.Events.FirstOrDefault()?.TimestampUtc.Dump() ?? "unknown",
            ["Z Timespan C-Last"] = group.Events.LastOrDefault()?.TimestampUtc.Dump() ?? "unknown"
        };

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
                Strong(logReader.Snapshot().IsPaused ? "paused" : "collecting"),
                ", toggle: ",
                Linker.LinkTo(HtmlEncode("▶"), InsightsPauseLogs.Link, more: "toggle=false"),
                " | ",
                Linker.LinkTo(HtmlEncode("⏸"), InsightsPauseLogs.Link, more: "toggle=true"),
                $" collecting {logReader.ListGroups().Length} retained log groups"
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
        var logItems = logReader.ListGroups()
            .Where(group => group.Segments.Contains(key, StringComparer.InvariantCultureIgnoreCase));
        if (!logItems.Any())
            return msg + "item not found";

        // Helper to check if any log has this key
        bool HasKey(string k) => logItems.Any(group => group.Specs.ContainsKey(k));

        // Helper to get the correct value depending of if it should fill the column, or it's found...
        //string GetValOrAlt(bool use, IDictionary<string, string> specs, string k) => !use 
        //    ? null 
        //    : specs?.TryGetValue(k, out var value) == true
        //        ? value
        //        : "";

        string GetVal(IReadOnlyDictionary<string, string?> specs, string k) =>
            specs.TryGetValue(k, out var value)
                ? value ?? ""
                : "";

        if (filter.HasValue())
        {
            var parts = filter.Split(',');
            foreach (var part in parts)
            {
                var criteria = part.Split('=');
                if (criteria.Length != 2) continue;
            logItems = logItems.Where(group =>
                group.Specs.TryGetValue(criteria[0], out var val) &&
                val?.EqualsInsensitive(criteria[1]) == true);
            }
        }
                

        var hasApp = HasKey(nameof(IAppIdentity.AppId));
        var hasSite = HasKey("SiteId");
        var hasPage = HasKey("PageId");
        var hasMod = HasKey("ModuleId");
        var hasUsr = HasKey("UserId");
        var totalSize = new SizeEstimate();
        var groups = logItems.ToArray();
        msg += P($"Logs Overview: {groups.Length}\n");
        msg += Table().Id("table").Wrap(
            HeadFields([
                "#",
                "Timestamp UTC",
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
            ]),
            Tbody(groups
                .Select((bundle, i) =>
                {
                    var firstIfExists = bundle.Events.FirstOrDefault();
                    var specs = bundle.Specs;
                    var timestamp = bundle.TimestampUtc.ToString("O").Substring(5);

                    var bestTitle = (bundle.Title ?? firstIfExists?.Message).NeverNull();

                    // Trim title if too long.
                    // Do NOT use .Ellipsis(...) because it tries to keep words together which will cut off too much of paths making it less useful.
                    var trimmedTitle = bestTitle.Length <= 150
                        ? bestTitle
                        : bestTitle.Substring(0, 150) + "…";

                    // Create the rows; note that any null values will be ignored and not create a cell
                    return RowFields([
                        $"{i + 1}",
                        Linker.LinkTo(timestamp, InsightsLogs.Link, key: key, more: $"position={i + 1}"),
                        !hasApp ? null : SpecialField.Right(GetVal(specs, nameof(IAppIdentity.AppId)), tooltip: GetVal(specs, "AppName") ),
                        !hasSite ? null : SpecialField.Right(GetVal(specs, "SiteId")),
                        !hasPage ? null : SpecialField.Right(GetVal(specs, "PageId")),
                        !hasMod ? null : SpecialField.Right(GetVal(specs, "ModuleId")),
                        !hasUsr ? null : SpecialField.Right(GetVal(specs, "UserId")),
                        SpecialField.Right($"{bundle.Events.Length:##,###}"),
                        SpecialField.Right("-"),
                        // WIP must find a slightly better way to truncate the title
                        SpecialField.Left(HtmlEncode(trimmedTitle), tooltip: bestTitle),
                        HtmlEncode(firstIfExists?.Result),
                        SpecialField.Right(ShowDuration(bundle.Events))
                    ]);
                })
                .ToArray<object>()));
        msg += "\n\n";
        msg += InsightsHtmlParts.JsTableSort();

        return msg;
    }

    private static string ShowDuration(IEnumerable<InsightsLogEventSnapshot> events)
        => events.Select(entry => entry.DurationMilliseconds).Where(value => value != null).Select(value => TimeSpan.FromMilliseconds(value!.Value)).DefaultIfEmpty().Max().ToString();

    internal string DumpTree(string title, InsightsLogGroupSnapshot group)
    {
        if (group.Events.Length == 0)
            return "";

        var log = new StringBuilder(H1(title) + Div(group.TimestampUtc.Dump()) + "\n\n<ol>");
        var depth = 0;
        // Legacy snapshots have wrap markers; MEL snapshots simply stay in provider sequence.
        foreach (var entry in group.Events)
        {
            if (entry.WrapClose)
            {
                if (depth > 0)
                {
                    log.AppendLine("</ol></li>");
                    depth--;
                }
                log.AppendLine($"<li>{SnapshotLine(entry)}</li>");
                continue;
            }

            log.AppendLine("<li>");
            log.AppendLine(SnapshotLine(entry));
            if (entry.WrapOpen)
            {
                log.AppendLine("<ol>");
                depth++;
            }
            else
                log.AppendLine("</li>");
        }
        while (depth-- > 0)
            log.AppendLine("</ol></li>");
        log.Append("</ol>end of log");
        return log.ToString();
    }

    private static string SnapshotLine(InsightsLogEventSnapshot entry)
    {
        var message = HtmlEncode(entry.Message.NeverNull());
        if (entry.ShowNewLines)
            message = Tags.Nl2Br(message).Replace("<br><br>", "<br>");
        var source = entry.FullSource ?? entry.ShortSource ?? entry.Category;
        var code = entry.SourceFilePath != null && !entry.HideCodeReference
            ? " " + HoverLabel("C#", $"{entry.SourceFilePath} - {entry.SourceMemberName}() #{entry.SourceLineNumber}", "codePeek")
            : "";
        var result = entry.Result == null ? "" : $" {ResStart}{HtmlEncode(entry.Result)}{ResEnd}";
        var exception = entry.Exception?.Details == null ? "" : " " + HtmlEncode(entry.Exception.Details);
        return Span(HoverLabel(HtmlEncode(entry.ShortSource ?? entry.Category), source, "logIds") + " - " + message + result + exception + code)
            .Class("log-line")
            .ToString();
    }

    internal string DumpTree(string title, ILog? log)
    {
        var typedLog = (Log)log!;
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
                if (breadcrumb.Count > 0)
                    breadcrumb.Pop();
                if (times.Count > 0)
                    times.Pop();
            }
            else
            {
                // Create an entry
                lg.AppendLine("<li>");
                var prevBreadcrumb = breadcrumb.Count > 0
                    ? breadcrumb.Peek()
                    : "";
                lg.AppendLine(TreeDumpOneLine(e, prevBreadcrumb, times.Count > 0
                    ? times.Peek()
                    : default, maxTimeSpan, startCreated));
                // 
                if (e.WrapOpen)
                {
                    // Go down one level
                    if (!e.WrapOpenWasClosed)
                        lg.AppendLine(HtmlEncode("🪵⚠️ LOGGER WARNING: This logger was never closed"));
                    breadcrumb.Push(e.ShortSource);
                    times.Push(e.Elapsed);
                    lg.AppendLine("<ol>");
                }
                else
                    lg.AppendLine("</li>");
            }
        }

        lg.Append("</ol>");
        lg.Append("end of log");
        return lg.ToString();
    }

    private static TimeSpan FullTimespan(Log log)
    {
        if (log.Entries.Count == 0)
            return default;
        var startTime = log.Created;

        var last = log.Entries.Last();
        var endTime = last.Created.Add(last.Elapsed);

        var timeSpan = endTime - startTime;
        return timeSpan;
    }

    /// <summary>
    /// If it has more than one segment, shorten it to last
    /// like Eav.Xyz[A7]Dyn.DnnCdr[62]Sxc.RzrHlp[93] => Sxc.RzrHlp[93]
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
        if (e.Options?.ShowNewLines == true)
            message = Tags.Nl2Br(message).Replace("<br><br>", "<br>");
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

    private string? _lastLogLabel;

}
