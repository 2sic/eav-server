﻿using System;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Plumbing;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys
{
    internal class InsightsTime
    {
        public InsightsTime(TimeSpan fullTime = default)
        {
            FullTime = fullTime;
        }

        private TimeSpan FullTime { get; }

        public string ShowTime(Entry e, TimeSpan parentTime)
        {
            if (e == null) return "";
            if (e.Elapsed == TimeSpan.Zero) return "";
            var seconds = e.Elapsed.TotalSeconds;
            var ms = e.Elapsed.TotalMilliseconds;
            var number = ms < 1000 ? ms : seconds;
            var time = number > 100
                ? $"{(int)number}"
                : $"{number:##.###}".Truncate(4).TrimEnd('.');

            time += (ms < 1000 ? "ms" : "s");

            // Figure out percent
            var parentPercent = PercentString1(ms, parentTime);
            if (parentPercent.HasValue()) parentPercent = " " + parentPercent;
            var fullPercent = PercentString1(ms, FullTime);
            if (fullPercent.HasValue()) fullPercent = $" | {Span(fullPercent).Class("time-of-total")}";
            var percentString = parentPercent + fullPercent;

            return Span(
                " ",
                Span(InsightsControllerReal.HtmlEncode("⌚")).Class("emoji"),
                $" {time}",
                percentString
            ).Class("time").ToString();
        }


        private static string PercentString1(double ownMs, TimeSpan compareTime)
        {
            const string notRelevant = "-";
            if (compareTime == default) return notRelevant;

            var compareMs = compareTime.TotalMilliseconds;
            var percent = ownMs / compareMs * 100;
            var percentString = percent < 0.5 ? notRelevant : $"{percent:##}%";

            return percentString;
        }
    }
}