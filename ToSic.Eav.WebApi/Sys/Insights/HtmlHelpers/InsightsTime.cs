using ToSic.Sys.Utils;
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
        
        var (secondsText, style) = PercentString(e, mainStart);

        if (e.Elapsed == TimeSpan.Zero)
            return Span(secondsText + " since " + HtmlEncode("▶️"))
                .Class("time")
                .Style(style)
                //.Style("text-align: right")
                .ToString();

        var seconds = e.Elapsed.TotalSeconds;
        var ms = e.Elapsed.TotalMilliseconds;
        var number = ms < 1000 ? ms : seconds;
        var time = number > 100
            ? $"{(int)number}"
            : $"{number:0.###}".Truncate(4).TrimEnd('.');

        time += ms < 1000 ? "ms" : "s";

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
    private (string secondsText, string style) PercentString(Entry e, DateTime mainStart)
    {
        var sinceStart = e.Created - mainStart;
        var secSinceStart = sinceStart.TotalSeconds;
        var secText = $"{secSinceStart:F}";
        var max = fullTime == default ? 5 : fullTime.TotalSeconds;
        var perSinceStart = (int)(secSinceStart / max * 100);

        var secElapsedSinceStart = e.Elapsed.TotalSeconds + secSinceStart;
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