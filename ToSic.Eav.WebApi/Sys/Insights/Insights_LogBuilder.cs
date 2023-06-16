using System.Linq;
using ToSic.Eav.WebApi.Sys.Insights;
using ToSic.Lib.Logging;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys
{
    public partial class InsightsControllerReal
    {

        //internal string LogHeader(string key, bool showFlush)
        //{
        //    var msg =
        //              +Div("back to " + LinkTo("2sxc insights home", nameof(Help)))
        //              + H1($"2sxc Insights: Log {key}")
        //              + P("Status: ",
        //                  Strong(_logStore.Pause ? "paused" : "collecting"),
        //                  ", toggle: ",
        //                  LinkTo(HtmlEncode("▶"), nameof(PauseLogs), more: "toggle=false"),
        //                  " | ",
        //                  LinkTo(HtmlEncode("⏸"), nameof(PauseLogs), more: "toggle=true"),
        //                  $" collecting #{_logStore.AddCount} of max {_logStore.MaxItems} (keep max {_logStore.SegmentSize} per set, then FIFO)"
        //                  + (showFlush ? " " + LinkTo("flush " + key, nameof(LogsFlush), key: key).ToString() : "")
        //              );
        //    return msg.ToString();
        //}

        //internal string LogHistoryOverview()
        //{
        //    var msg = "";
        //    try
        //    {
        //        var segments = _logStore.Segments;
        //        msg += P($"Logs Overview: {segments.Count}\n");

        //        var count = 0;

        //        msg += Table().Id("table").Wrap(
        //            InsightsHtmlTable.HeadFields("# ↕", "Key ↕", "Count ↕", "Actions ↕"),
        //            Tbody(
        //                segments.OrderBy(segPair => segPair.Key)
        //                    .Select(segPair => InsightsHtmlTable.RowFields((++count).ToString(),
        //                        LinkTo(segPair.Key, nameof(Logs), key: segPair.Key),
        //                        $"{segPair.Value.Count}",
        //                        LinkTo("flush", nameof(LogsFlush), key: segPair.Key))
        //                    )
        //                    .Cast<object>()
        //                    .ToArray())
        //        );
        //        msg += "\n\n";
        //        msg += InsightsHtmlParts.JsTableSort();
        //    }
        //    catch
        //    {
        //        // ignored
        //    }
        //    return msg;
        //}

        //internal string LogHistory(ILogStoreLive logStore, string key)
        //{
        //    var msg = "";
        //    if (!logStore.Segments.TryGetValue(key, out var set))
        //        return msg + "item not found";

        //    var count = 0;
        //    msg += P($"Logs Overview: {set.Count}\n");
        //    msg += Table().Id("table").Wrap(
        //        HeadFields("#", "Timestamp", "Key", "TopLevel Name", "Lines", "First Message", "Info", "Time"),
        //        Tbody(set
        //            .Select(bundle =>
        //            {
        //                var realLog = bundle.Log as Log;
        //                var firstIfExists = realLog?.Entries.FirstOrDefault();
        //                return RowFields(
        //                    $"{++count}",
        //                    realLog?.Created.ToUniversalTime().ToString("O"),
        //                    $"{key}",
        //                    LinkTo(realLog?.FullIdentifier, nameof(Logs), key: key, more: $"position={count}"),
        //                    $"{realLog?.Entries.Count}",
        //                    HtmlEncode((firstIfExists?.Message).NeverNull().Ellipsis(75, "…")),
        //                    HtmlEncode(firstIfExists?.Result),
        //                    new InsightsTime().ShowTime(realLog)
        //                );
        //            })
        //            .ToArray<object>()));
        //    msg += "\n\n";
        //    msg += InsightsHtmlParts.JsTableSort();


        //    return msg;
        //}

        //private const string ResStart = "<span style='color: green'>= ";
        //private const string ResEnd = "</span>";

        //internal static string DumpTree(string title, ILog log)
        //{
        //    var lg = new StringBuilder(
        //        H1($"{title}") +
        //        Div($"{(log as Log)?.Created.Dump()}") +
        //        "\n\n"
        //    );
        //    if ((log as Log)?.Entries.Count == 0) return "";
        //    lg.AppendLine("<ol>");

        //    var breadcrumb = new Stack<string>();
        //    var times = new Stack<TimeSpan>();

        //    var fullTime = (log as Log)?.Entries.FirstOrDefault()?.Elapsed ?? default;

        //    var time = new InsightsTime((log as Log)?.Entries.FirstOrDefault()?.Elapsed ?? default);
            
        //    foreach (var e in (log as Log)?.Entries)
        //    {
        //        // a wrap-close should happen before adding a line, since it must go up a level
        //        if (e.WrapClose)
        //        {
        //            // Go up one level
        //            lg.AppendLine("</ol></li>");
        //            if (breadcrumb.Count > 0) breadcrumb.Pop();
        //            if (times.Count > 0) times.Pop();
        //        }
        //        else
        //        {
        //            // Create an entry
        //            lg.AppendLine("<li>");
        //            lg.AppendLine(TreeDumpOneLine(e, breadcrumb.Count > 0 ? breadcrumb.Peek() : "", times.Count > 0 ? times.Peek() : default, time));
        //            // 
        //            if (e.WrapOpen)
        //            {
        //                // Go down one level
        //                if (!e.WrapOpenWasClosed) lg.AppendLine("LOGGER WARNING: This logger was never closed");
        //                breadcrumb.Push(e.ShortSource);
        //                times.Push(e.Elapsed);
        //                lg.AppendLine("<ol>");
        //            }
        //            else lg.AppendLine("</li>");
        //        }
        //    }

        //    lg.Append("</ol>");
        //    lg.Append("end of log");
        //    return lg.ToString();
        //}


        //private static string TreeDumpOneLine(Entry e, string parentName, TimeSpan parentTime, InsightsTime time)
        //{
        //    // if it's just a close, only repeat the result
        //    if (e.WrapClose)
        //        return $"{ResStart}{e.Result}{ResEnd}";

        //    #region find perfect Label

        //    var label = e.Source;
        //    if (!string.IsNullOrEmpty(parentName) && !string.IsNullOrEmpty(e.Source))
        //    {
        //        var foundParent = e.Source?.IndexOf(parentName) ?? 0;
        //        if (foundParent > 0)
        //        {
        //            var cut = foundParent + parentName.Length;
        //            if (!(label.Length > cut))
        //                cut = foundParent;
        //            label = e.Source.Substring(cut);
        //        }
        //    }

        //    #endregion

        //    var message = HtmlEncode(e.Message.NeverNull());
        //    if (e.Options?.ShowNewLines == true) message = Tags.Nl2Br(message).Replace("<br><br>", "<br>");
        //    return Span(
        //        HoverLabel(label, e.Source, "logIds")
        //           + " - "
        //           + message
        //           + (e.Result != null
        //               ? $" {ResStart}{HtmlEncode(e.Result)}{ResEnd}"
        //               : string.Empty)
        //           + time.ShowTime(e, parentTime)
        //           + (e.Code != null && e.Options?.HideCodeReference != true
        //               ? " " + HoverLabel("C#", $"{e.Code.Path} - {e.Code.Name}() #{e.Code.Line}", "codePeek")
        //               : string.Empty)
        //            + "\n")
        //        .Class("log-line")
        //        .ToString();
        //}
        
    }
}
