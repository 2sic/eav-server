namespace ToSic.Eav.Persistence.Efc;

partial class Efc11Loader
{

    public IDictionary<int, Zone> Zones()
    {
        var log = new Log("DB.EfLoad", null, "Zones()");
        // Add to zone-loading log, as it could
        logStore.Add(Lib.Logging.LogNames.LogStoreStartUp, log);
        var l = log.Fn<IDictionary<int, Zone>>(timer: true);

        // Build the tree of zones incl. their default(Content) and Primary apps
        var zones = context.ToSicEavZones
            .Include(z => z.ToSicEavApps)
            .Include(z => z.ToSicEavDimensions)
            .ThenInclude(d => d.ParentNavigation)
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
                    return new Zone(z.ZoneId, primary, content,
                        z.ToSicEavApps.ToDictionary(a => a.AppId, a => a.Name),
                        z.ToSicEavDimensions
                            .Where(d => d.ParentNavigation?.Key == Constants.CultureSystemKey)
                            .Cast<DimensionDefinition>().ToList());
                });
        return l.Return(zones, $"{zones.Count}");
    }
        
}