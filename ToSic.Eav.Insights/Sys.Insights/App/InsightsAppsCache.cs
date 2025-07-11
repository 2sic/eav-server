﻿using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.State;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Sys.Insights.Data;
using ToSic.Eav.Sys.Insights.HtmlHelpers;
using ToSic.Razor.Blade;
using static ToSic.Razor.Blade.Tag;

namespace ToSic.Eav.Sys.Insights.App;

internal class InsightsAppsCache(LazySvc<IAppsCatalog> appsCatalog, LazySvc<IAppStateCacheService> appStates, LazySvc<IAppReaderFactory> appReaders)
    : InsightsProvider(new() { Name = Link }, connect: [appsCatalog, appStates, appReaders])
{
    public static string Link = "AppsCache";

    public override string HtmlBody()
    {
        var msg = H1("Apps In Cache").ToString();

        var zones = appsCatalog.Value.Zones.OrderBy(z => z.Key);

        msg += "<table id='table'>"
               + InsightsHtmlTable.HeadFields([
                   "Zone ↕", "App ↕", AttributeNames.GuidNiceName, "InCache", "Name ↕", "Folder ↕", "Details",
                   "Actions", "Hash", "Timestamp", "List-Timestamp"
               ])
               + "<tbody>";

        foreach (var zone in zones)
        {
            var apps = zone.Value.Apps
                .Select(a =>
                {
                    var appIdentity = new AppIdentity(zone.Value.ZoneId, a.Key);
                    var inCache = appStates.Value.IsCached(appIdentity);
                    var appReader = inCache ? appReaders.Value.Get(appIdentity) : null;
                    var appState = inCache ? appStates.Value.Get(appIdentity) : null;
                    return new
                    {
                        Id = a.Key,
                        Guid = a.Value,
                        InCache = inCache,
                        Name = inCache
                            ? appReader?.Specs.Name ?? "unknown, app-infos not json"
                            : "not-loaded",
                        Folder = inCache
                            ? appReader?.Specs.Folder ?? "unknown, app-infos not json"
                            : "not-loaded",
                        Hash = appState?.GetHashCode(),
                        TS = appState?.CacheTimestamp,
                        ListTs = appState?.CacheTimestamp,
                    };
                })
                .OrderBy(a => a.Id);


            foreach (var app in apps)
            {
                msg += InsightsHtmlTable.RowFields([
                    zone.Key.ToString(),
                    app.Id.ToString(),
                    $"{app.Guid}",
                    app.InCache ? "yes" : "no",
                    app.Name,
                    app.Folder,
                    $"{Linker.LinkTo("stats", InsightsAppStats.Link, app.Id)} | {Linker.LinkTo("load log", InsightsAppLoadLog.Link, app.Id)} | {Linker.LinkTo("types", InsightsTypes.Link, app.Id)}",
                    Tag.Details(
                        Summary("show actions"),
                        app.Id != KnownAppsConstants.PresetAppId
                            ? Linker.LinkTo("purge", InsightsPurgeApp.Link, app.Id)
                            : null
                    ),
                    app.Hash?.ToString() ?? "-",
                    app.TS,
                    app.ListTs
                ]);
            }
        }
        msg += "</tbody>"
               + "</table>"
               + InsightsHtmlParts.JsTableSort();
        return msg;
    }
}