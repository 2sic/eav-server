namespace ToSic.Eav.Persistence.Efc;

internal class ZoneLoader(Efc11Loader parent): HelperBase(parent.Log, "Efc.ZoneLoader")
{
    public IDictionary<int, Zone> Zones(ILogStore logStore)
    {
        var log = new Log("DB.EfLoad", null, "Zones()");
        // Add to zone-loading log, as it could
        logStore.Add(LogNames.LogStoreStartUp, log);
        var l = log.Fn<IDictionary<int, Zone>>(timer: true);

        // Build the tree of zones incl. their default(Content) and Primary apps
        var lSql = log.Fn("Zone SQL", timer: true);
        var zonesSql = parent.Context.ToSicEavZones
            .Include(z => z.ToSicEavApps)
            .Include(z => z.ToSicEavDimensions)
            .ThenInclude(d => d.ParentNavigation)
            .ToList();
        lSql.Done($"Zones: {zonesSql.Count}");

        var zones = zonesSql
            .ToDictionary(
                z => z.ZoneId,
                z =>
                {
                    // On the default zone, the primary & default are the same
                    var primary = z.ZoneId == Constants.DefaultZoneId
                        ? Constants.MetaDataAppId
                        : z.ToSicEavApps.FirstOrDefault(a => a.Name == Constants.PrimaryAppGuid)?.AppId ?? -1;
                    var content = z.ZoneId == Constants.DefaultZoneId
                        ? Constants.MetaDataAppId
                        : z.ToSicEavApps.FirstOrDefault(a => a.Name == Constants.DefaultAppGuid)?.AppId ?? -1;

                    var appDictionary = z.ToSicEavApps.ToDictionary(a => a.AppId, a => a.Name);

                    var languages = z.ToSicEavDimensions
                        .Where(d => d.ParentNavigation?.Key == Constants.CultureSystemKey)
                        .Cast<DimensionDefinition>().ToList();

                    return new Zone(z.ZoneId, primary, content, appDictionary, languages);
                });
        return l.Return(zones, $"{zones.Count}");
    }

}