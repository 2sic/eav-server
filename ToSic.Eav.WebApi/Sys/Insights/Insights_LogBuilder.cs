﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToSic.Lib.Logging;

using ToSic.Eav.Plumbing;
using ToSic.Razor.Blade;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys
{
    public partial class InsightsControllerReal
    {

        internal string LogHeader(string key, bool showFlush)
        {
            var msg =
                      + Div("back to " + LinkTo("2sxc insights home", nameof(Help)))
                      + H1($"2sxc Insights: Log {key}")
                      + P("Status: ",
                          Strong(_logHistory.Pause ? "paused" : "collecting"),
                          ", toggle: ",
                          LinkTo(HtmlEncode("▶"), nameof(PauseLogs), more: "toggle=false"),
                          " | ",
                          LinkTo(HtmlEncode("⏸"), nameof(PauseLogs), more: "toggle=true"),
                          $" collecting #{_logHistory.Count} of max {_logHistory.MaxCollect} (keep max {_logHistory.Size} per set, then FIFO)"
                          + (showFlush ? " " + LinkTo("flush " + key, nameof(LogsFlush), key: key).ToString() : "")
                      );
            return msg.ToString();
        }

        internal string LogHistoryOverview(History logHistory)
        {
            var msg = "";
            try
            {
                var logs = logHistory.Logs;
                msg += P($"Logs Overview: {logs.Count}\n");

                var count = 0;

                msg += Table().Id("table").Wrap(
                    HeadFields("# ↕", "Key ↕", "Count ↕", "Actions ↕"),
                    Tbody(
                        logs.OrderBy(l => l.Key)
                            .Select(log => RowFields((++count).ToString(),
                                LinkTo(log.Key, nameof(Logs), key: log.Key),
                                $"{log.Value.Count}",
                                LinkTo("flush", nameof(LogsFlush), key: log.Key))
                            )
                            .Cast<object>()
                            .ToArray())
                );
                msg += "\n\n";
                msg += JsTableSort();
            }
            catch
            {
                // ignored
            }
            return msg;
        }

        internal string LogHistory(History logHistory, string key)
        {
            var msg = "";
            if (!logHistory.Logs.TryGetValue(key, out var set))
                return msg + "item not found";

            var count = 0;
            msg += P($"Logs Overview: {set.Count}\n");
            msg += Table().Id("table").Wrap(
                HeadFields("#", "Timestamp", "Key", "TopLevel Name", "Lines", "First Message", "Info", "Time"),
                Tbody(set
                    .Select(log =>
                    {
                        var firstIfExists = (log as Log)?.Entries.FirstOrDefault();
                        return RowFields(
                            $"{++count}",
                            (log as Log)?.Created.ToString("O"),
                            $"{key}",
                            LinkTo((log as Log)?.FullIdentifier, nameof(Logs), key: key, more: $"position={count}"),
                            $"{(log as Log)?.Entries.Count}",
                            HtmlEncode((firstIfExists?.Message).NeverNull().Ellipsis(75, "…")),
                            HtmlEncode(firstIfExists?.Result),
                            new InsightsTime().ShowTime(firstIfExists, default)
                        );
                    })
                    .ToArray<object>()));
            msg += "\n\n";
            msg += JsTableSort();


            return msg;
        }

        private const string ResStart = "<span style='color: green'>= ";
        private const string ResEnd = "</span>";

        internal static string DumpTree(string title, ILog log)
        {
            var lg = new StringBuilder(H1($"{title}") + "\n\n");
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

            return Span(
                HoverLabel(label, e.Source, "logIds")
                   + " - "
                   + HtmlEncode(e.Message)
                   + (e.Result != null
                       ? $"{ResStart}{HtmlEncode(e.Result)}{ResEnd}"
                       : string.Empty)
                   + time.ShowTime(e, parentTime)
                   + (e.Code != null
                       ? " " + HoverLabel("C#", $"{e.Code.Path} - {e.Code.Name}() #{e.Code.Line}", "codePeek")
                       : string.Empty)
                    + "\n")
                .Class("log-line")
                .ToString();
        }
        
    }
}
