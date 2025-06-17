namespace ToSic.Eav.Sys.Insights.Logs;

internal class InsightsLogsFlush(LazySvc<ILogStoreLive> logStore)
    : InsightsProvider(new() { Name = Link, Title = "Flush Logs" }, connect: [logStore])
{
    public static string Link = "LogsFlush";

    public override string HtmlBody()
        => Key == null
            ? "error: please add key to the url parameters"
            : LogsFlush(Key);

    internal string LogsFlush(string key)
    {
        Log.A($"flush log for {key}");
        logStore.Value.FlushSegment(key);
        return $"flushed log history for {key}";
    }
}