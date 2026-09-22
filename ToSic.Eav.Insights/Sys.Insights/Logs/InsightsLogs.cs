using ToSic.Eav.Sys.Insights.HtmlHelpers;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.Logs;

internal class InsightsLogs : InsightsProvider
{
    public static string Link = "Logs";

    private readonly LazySvc<IInsightsLogSnapshotReader> _snapshotReader;
    private readonly LazySvc<ILogStore> _logStore;

    public InsightsLogs(LazySvc<IInsightsLogSnapshotReader> snapshotReader, LazySvc<ILogStore> logStore) : base(new() { Name = Link, Teaser = "Logs of Modules, APIs and more", HelpCategory = "Logging", Title = "Insights into Logs" }, connect: [snapshotReader, logStore])
    {
        _snapshotReader = snapshotReader;
        _logStore = logStore;
        BootLog.AddToStore(logStore.Value);
    }

    public override string HtmlBody()
        => Key == null
            ? Logs()
            : Position == null
                ? Logs(Key, Filter)
                : Logs(Key, Position.Value);

    internal string Logs()
    {
        Log.A("debug log load");
        var logHtml = new InsightsLogsHelper(_snapshotReader.Value.Snapshot());
        return logHtml.LogHeader("Overview", false)
               + logHtml.LogHistoryOverview();
    }

    private string Logs(string key, string? filter)
    {
        Log.A($"debug log load for {key}");
        var logHtml = new InsightsLogsHelper(_snapshotReader.Value.Snapshot());
        return logHtml.LogHeader(key, true, filter.HasValue())
               + logHtml.LogHistoryList(key, filter!);
    }

    private string Logs(string key, int position)
    {
        Log.A($"debug log load for {key}/{position}");
        var snapshot = _snapshotReader.Value.Snapshot();
        var logHtml = new InsightsLogsHelper(snapshot);
        var msg = InsightsHtmlParts.PageStyles() + logHtml.LogHeader($"{key}[{position}]", false);

        // Read one immutable group list for this page, so positions cannot drift while rendering.
        var set = snapshot.Groups
            .Where(group => group.Segments.Contains(key, StringComparer.InvariantCultureIgnoreCase))
            .ToArray();
        if (set.Length == 0)
            return msg + $"position {position} not found in log set {key}";

        if (position < 1 || set.Length < position)
            return msg + $"position ({position}) > count ({set.Length})";

        var bundle = set[position - 1];

        return msg + logHtml.ShowSpecs(bundle) + logHtml.DumpTree($"Log for {key}[{position}]", bundle);
    }


}
