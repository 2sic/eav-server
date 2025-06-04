using ToSic.Eav.Apps.Sys.State;
using ToSic.Razor.Blade;
using ToSic.Sys.Caching;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.App;

internal class InsightsAppStats(IAppStateCacheService appStates) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [appStates])
{
    public static string Link = "AppStats";

    public override string HtmlBody()
    {
        var appId = AppId;
        if (UrlParamsIncomplete(appId, out var message))
            return message;

        Log.A($"debug app-internals for {appId}");

        var pkg = appStates.Get(appId.Value);

        var msg = H1($"App internals for {appId}").ToString();
        try
        {
            Log.A("general stats");
            msg += P(
                Tags.Nl2Br($"AppId: {pkg.AppId}\n"
                           + $"Timestamp First : {pkg.CacheStatistics.FirstTimestamp} = {pkg.CacheStatistics.FirstTimestamp.ToReadable()}\n"
                           + $"Timestamp Current: {pkg.CacheStatistics.CacheTimestamp} = {pkg.CacheStatistics.CacheTimestamp.ToReadable()}\n"
                           + $"Update Count: {pkg.CacheStatistics.ResetCount}\n"
                           + $"Dyn Update Count: {pkg.DynamicUpdatesCount}\n"
                           + "\n")
            );

            msg += H2("History");

            msg += Ol(
                pkg.CacheStatistics.History
                    .Reverse()
                    .Select(timestamp => Li($"Resets: {timestamp.ResetCount}; Items: {timestamp.ItemCount}; TS: {timestamp.Timestamp} = {timestamp.Timestamp.ToReadable()}"))
                    .ToArray<object>()
            );
        }
        catch { /* ignore */ }

        return msg;
    }
}