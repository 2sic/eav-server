﻿using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Data.Dimensions.Sys;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Persistence.Efc.Sys.Services;

internal class EfcZoneLoaderService(EfcAppLoaderService appLoader): HelperBase(appLoader.Log, "Efc.ZoneLoader")
{
    internal IDictionary<int, Zone> LoadZones(ILogStore logStore)
    {
        var log = new Log("DB.EfLoad", null, "Zones()");
        // Add to zone-loading log, as it could
        logStore.Add(LogNames.LogStoreStartUp, log);
        var l = log.Fn<IDictionary<int, Zone>>(timer: true);

        // Build the tree of zones incl. their default(Content) and Primary apps
        var lSql = log.Fn("Zone SQL", timer: true);
        var zonesSql = appLoader.Context.TsDynDataZones
            .Include(z => z.TsDynDataApps)
            .Include(z => z.TsDynDataDimensions)
            .ThenInclude(d => d.ParentNavigation)
            .ToList();
        lSql.Done($"Zones: {zonesSql.Count}");

        var zones = zonesSql
            .ToDictionary(
                z => z.ZoneId,
                z =>
                {
                    // On the default zone, the primary & default are the same
                    var primary = z.ZoneId == KnownAppsConstants.DefaultZoneId
                        ? KnownAppsConstants.MetaDataAppId
                        : z.TsDynDataApps.FirstOrDefault(a => a.Name == KnownAppsConstants.PrimaryAppGuid)?.AppId ?? -1;
                    var content = z.ZoneId == KnownAppsConstants.DefaultZoneId
                        ? KnownAppsConstants.MetaDataAppId
                        : z.TsDynDataApps.FirstOrDefault(a => a.Name == KnownAppsConstants.DefaultAppGuid)?.AppId ?? -1;

                    var appDictionary = z.TsDynDataApps.ToDictionary(a => a.AppId, a => a.Name);

                    var languages = z.TsDynDataDimensions
                        .Where(d => d.ParentNavigation?.Key == EavConstants.CultureSystemKey)
                        .Select(d => d.AsDimensionDefinition)
                        .ToList();

                    return new Zone(z.ZoneId, primary, content, appDictionary, languages);
                });
        return l.Return(zones, $"{zones.Count}");
    }

}