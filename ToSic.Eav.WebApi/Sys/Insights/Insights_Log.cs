using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

partial class InsightsControllerReal
{
    internal string Logs()
    {
        Log.A("debug log load");
        return _logHtml.LogHeader("Overview", false) + _logHtml.LogHistoryOverview();
    }

    private string Logs(string key, string filter)
    {
        Log.A($"debug log load for {key}");
        return _logHtml.LogHeader(key, true, filter.HasValue()) + _logHtml.LogHistoryList(key, filter);
    }

    private string Logs(string key, int position)
    {
        Log.A($"debug log load for {key}/{position}");
        var msg = InsightsHtmlParts.PageStyles() + _logHtml.LogHeader($"{key}[{position}]", false);

        if (!_logStore.Segments.TryGetValue(key, out var set))
            return msg + $"position {position} not found in log set {key}";

        if (set.Count < position - 1)
            return msg + $"position ({position}) > count ({set.Count})";

        var bundle = set.Take(position).LastOrDefault();

        return msg + (bundle?.Log == null
            ? P("log is null").ToString()
            : _logHtml.ShowSpecs(bundle) + _logHtml.DumpTree($"Log for {key}[{position}]", bundle.Log));
    }

    internal string PauseLogs(bool pause)
    {
        Log.A($"pause log {pause}");
        _logStore.Pause = pause;
        return $"pause set to {pause}";
    }

    internal string LogsFlush(string key)
    {
        Log.A($"flush log for {key}");
        _logStore.FlushSegment(key);
        return $"flushed log history for {key}";
    }
}