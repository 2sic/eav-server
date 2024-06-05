using ToSic.Eav.Apps.Internal.Insights;
using ToSic.Eav.Caching;
using ToSic.Razor.Blade;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.WebApi.Sys.Insights;

internal class InsightsAppStats(IAppStates appStates) : InsightsProvider(Link, helpCategory: HiddenFromAutoDisplay, connect: [appStates])
{
    public static string Link = "AppStats";

    public override string HtmlBody()
    {
        var appId = AppId;
        if (UrlParamsIncomplete(appId, out var message))
            return message;

        Log.A($"debug app-internals for {appId}");

        var pkg = appStates.GetCacheState(appId.Value);

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