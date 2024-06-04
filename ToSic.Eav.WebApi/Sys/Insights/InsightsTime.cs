using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsTime(TimeSpan fullTime = default) : InsightsHtmlBase
{
    private TimeSpan FullTime => fullTime;

    public string ShowTime(Log log)
    {
        if (log == null) return "";
        var firstWithTime = log.Entries.FirstOrDefault(e => e.Elapsed != TimeSpan.Zero);
        return ShowTime(firstWithTime, default, log.Created);
    }

    public string ShowTime(Entry e, TimeSpan parentTime, DateTime mainStart)
    {
        if (e == null) return "";
        
        var (_, secondsText, style) = PercentString(e, mainStart);

        if (e.Elapsed == TimeSpan.Zero)
            return e.WrapOpen
                ? Span(secondsText)
                    .Class("time")
                    .Style(style)
                    .Style("text-align: right")
                    .ToString()
                : "";

        var seconds = e.Elapsed.TotalSeconds;
        var ms = e.Elapsed.TotalMilliseconds;
        var number = ms < 1000 ? ms : seconds;
        var time = number > 100
            ? $"{(int)number}"
            : $"{number:##.###}".Truncate(4).TrimEnd('.');

        time += (ms < 1000 ? "ms" : "s");

        // Figure out percent of parent total time
        var parentPercent = PercentString1(ms, parentTime);
        if (parentPercent.HasValue()) parentPercent = " " + parentPercent;
        var fullPercent = PercentString1(ms, FullTime);
        if (fullPercent.HasValue()) fullPercent = $" | {Span(fullPercent).Class("time-of-total")}";
        var percentString = parentPercent + fullPercent;

        return Span(
                " ",
                Span(HtmlEncode("⌚")).Class("emoji"),
                $" {time}",
                percentString,
                " | " + secondsText
            )
            .Class("time")
            .Style(style)
            .ToString();
    }

    /// <summary>
    /// figure out time / % since start
    /// </summary>
    /// <param name="e"></param>
    /// <param name="mainStart"></param>
    /// <returns></returns>
    private (int percent, string secondsText, string style) PercentString(Entry e, DateTime mainStart)
    {
        var sinceStart = e.Created - mainStart;
        // var msSinceStart = sinceStart.TotalMilliseconds;
        var secondsSinceStart = sinceStart.TotalSeconds;
        var secondsText = $"{secondsSinceStart:F}";
        var max = fullTime == default ? 5 : fullTime.TotalSeconds;
        var percentSinceStart = (int)(secondsSinceStart / max * 100);
        var style = $"background: linear-gradient(90deg, #e9dfe0 {100 - percentSinceStart}%, #b9b8ff 0)";
        return (percentSinceStart, secondsText, style);
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