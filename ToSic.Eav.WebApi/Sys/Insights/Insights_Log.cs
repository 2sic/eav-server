using System.Linq;
using ToSic.Lib.Logging;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys
{
    public partial class InsightsControllerReal
    {
        private string Logs()
        {
            Log.A("debug log load");
            return LogHeader("Overview", false) + LogHistoryOverview(_logHistory);
        }

        private string Logs(string key)
        {
            Log.A($"debug log load for {key}");
            return LogHeader(key, true) + LogHistory(_logHistory, key);
        }

        private string Logs(string key, int position)
        {
            Log.A($"debug log load for {key}/{position}");
            var msg = PageStyles() + LogHeader($"{key}[{position}]", false);

            if (!_logHistory.Segments.TryGetValue(key, out var set))
                return msg + $"position {position} not found in log set {key}";

            if (set.Count < position - 1)
                return msg + $"position ({position}) > count ({set.Count})";
            
            var log = set.Take(position).LastOrDefault();
            return msg + (log == null
                ? P("log is null").ToString()
                : DumpTree($"Log for {key}[{position}]", log));
        }

        private string PauseLogs(bool pause)
        {
            Log.A($"pause log {pause}");
            _logHistory.Pause = pause;
            return $"pause set to {pause}";
        }

        private string LogsFlush(string key)
        {
            Log.A($"flush log for {key}");
            _logHistory.FlushSegment(key);
            return $"flushed log history for {key}";
        }
    }
}
