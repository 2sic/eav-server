namespace ToSic.Eav.Sys.Insights.Logs;

internal class InsightsPauseLogs(LazySvc<ILogStoreLive> logStore) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay , connect: [logStore])
{
    public static string Link = "PauseLogs";

    public override string Title => "Pause Logs";

    public override string HtmlBody()
        => PauseLogs(Toggle ?? true);

    internal string PauseLogs(bool pause)
    {
        Log.A($"pause log {pause}");
        logStore.Value.Pause = pause;
        return $"pause set to {pause}";
    }
}