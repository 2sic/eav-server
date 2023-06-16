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

            var specList = Table(Tr(Th("Aspect"), Th("Value")));
            specList = specs.Aggregate(specList, (current, spec) 
                => current.Add(Tr(Td(spec.Key), Td(spec.Value))));

            return Div(H2("Log Specs"), specList);
        }

        internal string LogHeader(string key, bool showFlush)
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
                    + (showFlush ? " " + LinkTo("flush " + key, nameof(InsightsControllerReal.LogsFlush), key: key).ToString() : "")
                );
            return msg.ToString();
        }


        internal string LogHistoryList(ILogStoreLive logStore, string key)
        {
            var msg = "";
            if (!logStore.Segments.TryGetValue(key, out var set))
                return msg + "item not found";

            // Helper to check if any log has this key
            bool HasKey(string k) => set.Any(s => s.Specs?.ContainsKey(k) == true);

            // Helper to get the correct value depending of if it should fill the column, or it's found...
            string GetValOrAlt(bool use, IDictionary<string, string> specs, string k) => !use ? null : specs.TryGetValue(k, out var a) ? a : "";

            var hasApp = HasKey(nameof(IAppIdentity.AppId));
            var hasSite = HasKey("SiteId");
            var hasPage = HasKey("PageId");
            var hasMod = HasKey("ModuleId");
            msg += P($"Logs Overview: {set.Count}\n");
            msg += Table().Id("table").Wrap(
                HeadFields("#", "Timestamp",
                    hasApp ? "App" : null,
                    hasSite ? "Site": null,
                    hasPage ? "Page": null,
                    hasMod ? "Mod": null,
                    /*"Key",*/ /*"TopLevel Name",*/
                    "Lines", "First Message", "Info", "Time"),
                Tbody(set
                    .Select((bundle, i) =>
                    {
                        var realLog = bundle.Log as Log;
                        var firstIfExists = realLog?.Entries.FirstOrDefault();
                        var specs = bundle.Specs;
                        var timestamp = realLog?.Created.ToUniversalTime().ToString("O").Substring(5) ?? "no timestamp";
                        return RowFields(
                            $"{i+1}",
                            LinkTo(timestamp, nameof(InsightsControllerReal.Logs), key: key, more: $"position={i+1}"),
                            GetValOrAlt(hasApp, specs, nameof(IAppIdentity.AppId)),
                            GetValOrAlt(hasSite, specs, "Site"),
                            GetValOrAlt(hasPage, specs, "PageId"),
                            GetValOrAlt(hasMod, specs, "Module"),
                            $"{realLog?.Entries.Count}",
                            HtmlEncode((firstIfExists?.Message).NeverNull().Ellipsis(75, "…")),
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

            var fullTime = (log as Log)?.Entries.FirstOrDefault()?.Elapsed ?? default;

            var time = new InsightsTime((log as Log)?.Entries.FirstOrDefault()?.Elapsed ?? default);

            foreach (var e in (log as Log)?.Entries)
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
                    lg.AppendLine(TreeDumpOneLine(e, breadcrumb.Count > 0 ? breadcrumb.Peek() : "", times.Count > 0 ? times.Peek() : default, time));
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

        private static string TreeDumpOneLine(Entry e, string parentName, TimeSpan parentTime, InsightsTime time)
        {
            // if it's just a close, only repeat the result
            if (e.WrapClose)
                return $"{ResStart}{e.Result}{ResEnd}";

            #region find perfect Label

            var label = e.Source;
            if (!string.IsNullOrEmpty(parentName) && !string.IsNullOrEmpty(e.Source))
            {
                var foundParent = e.Source?.IndexOf(parentName) ?? 0;
                if (foundParent > 0)
                {
                    var cut = foundParent + parentName.Length;
                    if (!(label.Length > cut))
                        cut = foundParent;
                    label = e.Source.Substring(cut);
                }
            }

            #endregion

            var message = HtmlEncode(e.Message.NeverNull());
            if (e.Options?.ShowNewLines == true) message = Tags.Nl2Br(message).Replace("<br><br>", "<br>");
            return Span(
                    HoverLabel(label, e.Source, "logIds")
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


    }
}
