﻿using ToSic.Eav.Sys.Insights.HtmlHelpers;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.Logs;

internal class InsightsLogs : InsightsProvider
{
    public static string Link = "Logs";

    [field: AllowNull, MaybeNull]
    private InsightsLogsHelper LogHtml => field ??= new(_logStore.Value);
    private readonly LazySvc<ILogStoreLive> _logStore;

    public InsightsLogs(LazySvc<ILogStoreLive> logStore) : base(new() { Name = Link, Teaser = "Logs of Modules, APIs and more", HelpCategory = "Logging", Title = "Insights into Logs" }, connect: [logStore])
    {
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
        return LogHtml.LogHeader("Overview", false)
               + LogHtml.LogHistoryOverview();
    }

    private string Logs(string key, string? filter)
    {
        Log.A($"debug log load for {key}");
        return LogHtml.LogHeader(key, true, filter.HasValue())
               + LogHtml.LogHistoryList(key, filter!);
    }

    private string Logs(string key, int position)
    {
        Log.A($"debug log load for {key}/{position}");
        var msg = InsightsHtmlParts.PageStyles() + LogHtml.LogHeader($"{key}[{position}]", false);

        if (!_logStore.Value.Segments.TryGetValue(key, out var set))
            return msg + $"position {position} not found in log set {key}";

        if (set.Count < position - 1)
            return msg + $"position ({position}) > count ({set.Count})";

        var bundle = set.Take(position).LastOrDefault();

        return msg + (bundle?.Log == null
            ? P("log is null").ToString()
            : LogHtml.ShowSpecs(bundle) + LogHtml.DumpTree($"Log for {key}[{position}]", bundle.Log));
    }


}