namespace ToSic.Eav.Sys.Insights.Logs;

internal class InsightsPauseLogs(LazySvc<IInsightsLogSnapshotReader> snapshotReader)
    : InsightsProvider(new() { Name = Link, Title = "Pause Logs" }, connect: [snapshotReader])
{
    public static string Link = "PauseLogs";

    public override string HtmlBody()
        => PauseLogs(Toggle ?? true);

    internal string PauseLogs(bool pause)
    {
        Log.A($"pause log {pause}");
        if (pause) snapshotReader.Value.Pause();
        else snapshotReader.Value.Resume();
        return $"pause set to {pause}";
    }
}
