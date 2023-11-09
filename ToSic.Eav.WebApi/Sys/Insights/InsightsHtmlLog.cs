using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToSic.Eav.Apps;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Logging;
using ToSic.Razor.Blade;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights
{
    internal class InsightsHtmlLog: InsightsHtmlTable
    {
        private readonly ILogStoreLive _logStore;
        public InsightsHtmlLog(ILogStoreLive logStore)
        {
            _logStore = logStore;
        }

        internal string LogHistoryOverview()
        {
            var msg = "";
            try
            {
                var segments = _logStore.Segments;
                msg += P($"Logs Overview: {segments.Count}\n");

                var count = 0;

                msg += Table().Id("table").Wrap(
                    HeadFields("# ↕", "Key ↕", "Count ↕", "Actions ↕"),
                    Tbody(
                        segments.OrderBy(segPair => segPair.Key)
                            .Select(segPair => RowFields((++count).ToString(),
                                LinkTo(segPair.Key, nameof(InsightsControllerReal.Logs), key: segPair.Key),
                                $"{segPair.Value.Count}",
                                LinkTo("flush", nameof(InsightsControllerReal.LogsFlush), key: segPair.Key))
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


        public IHtmlTag ShowSpecs(LogStoreEntry entry)
        {
            var specs = entry?.Specs;
            if (specs == null) return null;

            var specList = Table(HeadFields(SpecialField.Left("Aspect ↕"), SpecialField.Left("Value ↕")));
            specList = specs
                .OrderBy(s => s.Key)
                .Aggregate(specList, (current, spec) 
                => current.Add(Tr(Td(spec.Key), Td(spec.Value))));

            return Div(H2("Log Specs"), specList);
        }

        internal string LogHeader(string key, bool showFlush, bool showReset = false)
        {
            var msg =
                +Div("back to " + LinkTo("2sxc insights home", nameof(InsightsControllerReal.Help)))
                + H1($"2sxc Insights: Log {key}")
                + P("Status: ",
                    Strong(_logStore.Pause ? "paused" : "collecting"),
                    ", toggle: ",
                    LinkTo(HtmlEncode("▶"), nameof(InsightsControllerReal.PauseLogs), more: "toggle=false"),
                    " | ",
                    LinkTo(HtmlEncode("⏸"), nameof(InsightsControllerReal.PauseLogs), more: "toggle=true"),
                    $" collecting #{_logStore.AddCount} of max {_logStore.MaxItems} (keep max {_logStore.SegmentSize} per set, then FIFO)"
                    + (showFlush
                        ? " " + LinkTo("flush " + key, nameof(InsightsControllerReal.LogsFlush), key: key).ToString()
                        : "")
                );
            if (showReset)
                msg += Br()
                       + Strong("This list has filters applied. ")
                       + LinkTo(HtmlEncode("❌") + "remove filters", nameof(InsightsControllerReal.Logs), key: key);
                
            return msg.ToString();
        }


        internal string LogHistoryList(string key, string filter)
        {
            var msg = "";
            if (!_logStore.Segments.TryGetValue(key, out var set))
                return msg + "item not found";

            // Helper to check if any log has this key
            bool HasKey(string k) => set.Any(s => s.Specs?.ContainsKey(k) == true);

            // Helper to get the correct value depending of if it should fill the column, or it's found...
            string GetValOrAlt(bool use, IDictionary<string, string> specs, string k) => !use 
                ? null 
                : specs?.TryGetValue(k, out var a) == true ? a : "";

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
            msg += P($"Logs Overview: {set.Count}\n");
            msg += Table().Id("table").Wrap(
                HeadFields("#", "Timestamp",
                    hasApp ? "App ↕" : null,
                    hasSite ? "Site ↕" : null,
                    hasPage ? "Page ↕" : null,
                    hasMod ? "Mod ↕" : null,
                    "Lines", "First Message", "Info", "Time"),
                Tbody(logItems
                    .Select((bundle, i) =>
                    {
                        var realLog = bundle.Log as Log;
                        var firstIfExists = realLog?.Entries.FirstOrDefault();
                        var specs = bundle.Specs;
                        var timestamp = realLog?.Created.ToUniversalTime().ToString("O").Substring(5) ?? "no timestamp";
                        return RowFields(
                            $"{i + 1}",
                            LinkTo(timestamp, nameof(InsightsControllerReal.Logs), key: key, more: $"position={i + 1}"),
                            GetValOrAlt(hasApp, specs, nameof(IAppIdentity.AppId)),
                            GetValOrAlt(hasSite, specs, "SiteId"),
                            GetValOrAlt(hasPage, specs, "PageId"),
                            GetValOrAlt(hasMod, specs, "ModuleId"),
                            $"{realLog?.Entries.Count}",
                            HtmlEncode((firstIfExists?.Message).NeverNull().Ellipsis(150, "…")),
                            HtmlEncode(firstIfExists?.Result),
                            new InsightsTime().ShowTime(realLog)
                        );
                    })
                    .ToArray<object>()));
            msg += "\n\n";
            msg += InsightsHtmlParts.JsTableSort();

            return msg;
        }

        internal string DumpTree(string title, ILog log)
        {
            var lg = new StringBuilder(
                H1($"{title}") +
                Div($"{(log as Log)?.Created.Dump()}") +
                "\n\n"
            );
            if ((log as Log)?.Entries.Count == 0) return "";
            lg.AppendLine("<ol>");

            var breadcrumb = new Stack<string>();
            var times = new Stack<TimeSpan>();

            var entries = (log as Log)?.Entries;
            var time = new InsightsTime(entries?.FirstOrDefault()?.Elapsed ?? default);

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
                    lg.AppendLine(TreeDumpOneLine(e, prevBreadcrumb, times.Count > 0 ? times.Peek() : default, time));
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

        private string TreeDumpOneLine(Entry e, string parentBreadcrumb, TimeSpan parentTime, InsightsTime time)
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
                    + time.ShowTime(e, parentTime)
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
}
