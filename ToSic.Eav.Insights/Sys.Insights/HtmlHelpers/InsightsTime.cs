using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.HtmlHelpers;

internal class InsightsTime(TimeSpan fullTime = default) : InsightsHtmlBase
{
    private TimeSpan FullTime => fullTime;

    public string ShowTime(Log? log)
    {
        if (log == null) return "";
        var firstWithTime = log.Entries.FirstOrDefault(e => e.Elapsed != TimeSpan.Zero);
        return ShowTime(firstWithTime, default, log.Created);
    }

    public string ShowTime(Entry? e, TimeSpan parentTime, DateTime mainStart)
        => e == null ? "" : ShowTime(e.Elapsed, e.Created, parentTime, mainStart);

    public string ShowTime(TimeSpan elapsed, DateTime created, TimeSpan parentTime, DateTime mainStart)
    {
        var (secondsText, style) = PercentString(elapsed, created, mainStart);

        if (elapsed == TimeSpan.Zero)
            return Span(secondsText + " since " + HtmlEncode("▶️"))
                .Class("time")
                .Style(style)
                //.Style("text-align: right")
                .ToString();

        var ms = elapsed.TotalMilliseconds;

        // Figure out percent of parent total time
        var parentPercent = PercentString1(ms, parentTime);
        if (parentPercent.HasValue()) parentPercent = " " + parentPercent;
        var fullPercent = PercentString1(ms, FullTime);
        if (fullPercent.HasValue()) fullPercent = $" | {Span(fullPercent).Class("time-of-total")}";
        var percentString = parentPercent + fullPercent;

        return DurationBadge(elapsed, percentString + " | " + secondsText, style);
    }

    // Snapshot durations do not imply a known start time or parent relationship.
    public string ShowTime(TimeSpan elapsed)
        => DurationBadge(elapsed, "", $"background: {ColorElapsed}");

    public string ShowTime(TimeSpan? elapsed, TimeSpan sinceStart)
    {
        var secondsText = $"{Math.Max(0, sinceStart.TotalSeconds):F}";
        return elapsed is { } duration
            ? DurationBadge(duration, " | " + secondsText, $"background: {ColorElapsed}")
            : Span(secondsText + " since " + HtmlEncode("▶️")).Class("time").ToString();
    }

    private static string DurationBadge(TimeSpan elapsed, string suffix, string style)
    {
        var seconds = elapsed.TotalSeconds;
        var ms = elapsed.TotalMilliseconds;
        var number = ms < 1000 ? ms : seconds;
        // Keep short calls visible instead of rounding sub-millisecond work to 0ms.
        var time = ms is > 0 and < 1
            ? $"{ms:0.####}"
            : number > 100
                ? $"{(int)number}"
                : $"{number:0.###}".Truncate(4)!.TrimEnd('.');

        time += ms < 1000 ? "ms" : "s";

        return Span(
                " ",
                Span(HtmlEncode("⌚")).Class("emoji"),
                $" {time}",
                suffix
            )
            .Class("time")
            .Style(style)
            .ToString();
    }

    /// <summary>
    /// figure out time / % since start
    /// </summary>
    /// <param name="elapsed"></param>
    /// <param name="created"></param>
    /// <param name="mainStart"></param>
    /// <returns></returns>
    private (string secondsText, string style) PercentString(TimeSpan elapsed, DateTime created, DateTime mainStart)
    {
        var sinceStart = created - mainStart;
        var secSinceStart = sinceStart.TotalSeconds;
        var secText = $"{secSinceStart:F}";
        var max = fullTime == default ? 5 : fullTime.TotalSeconds;
        var perSinceStart = (int)(secSinceStart / max * 100);

        var secElapsedSinceStart = elapsed.TotalSeconds + secSinceStart;
        var perElapsed = (int)(secElapsedSinceStart / max * 100);


        var style = $"background: linear-gradient(90deg, {ColorBefore} {perSinceStart}%, {ColorElapsed} {perSinceStart}%, {ColorElapsed} {perElapsed}%, {ColorRemaining} {perElapsed}%)";
        return (secText, style);
    }

    private const string ColorRemaining = "#eeeeee";
    private const string ColorBefore = "#bbbbbb";
    private const string ColorElapsed = "#b9b8ff";


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
