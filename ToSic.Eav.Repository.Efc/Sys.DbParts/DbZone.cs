using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

internal class DbZone(DbStorage.DbStorage dbStore) : HelperBase(dbStore.LogDetails, "Db.Zone")
{
    private ILog? LogSummary => dbStore.LogSummary == null ? null : field ??= Log;

    /// <summary>
    /// Creates a new Zone with a default App and Culture-Root-Dimension
    /// </summary>
    public int AddZone(string name)
    {
        var newZone = new TsDynDataZone { Name = name };
        dbStore.SqlDb.Add(newZone);

        dbStore.Dimensions.AddRootCultureNode(EavConstants.CultureSystemKey, "Culture Root", newZone);

        dbStore.App.AddAppAndSave(newZone.ZoneId, KnownAppsConstants.DefaultAppGuid);

        // We reliably auto-init the site-app by default
        dbStore.App.AddAppAndSave(newZone.ZoneId, KnownAppsConstants.PrimaryAppGuid);

        return newZone.ZoneId;
    }



    public bool AddMissingPrimaryApps()
    {
        var l = LogSummary.Fn<bool>();

        var zonesWithoutPrimary = dbStore.SqlDb.TsDynDataZones
            .Include(z => z.TsDynDataApps)
            .Include(z => z.TsDynDataDimensions)
            // Skip "default" zone as that is a single purpose technical zone
            .Where(z => z.ZoneId != KnownAppsConstants.DefaultZoneId)
            .Where(z => z.TsDynDataApps.All(a => a.Name != KnownAppsConstants.PrimaryAppGuid))
            .ToList();

        if (!zonesWithoutPrimary.Any()) return l.ReturnFalse("no missing primary");

        var newZones = zonesWithoutPrimary
            .Select(zone => new TsDynDataApp
            {
                Name = KnownAppsConstants.PrimaryAppGuid,
                Zone = zone
            }).ToList();

        dbStore.DoAndSaveWithoutChangeDetection(() => dbStore.SqlDb.AddRange(newZones));

        return l.ReturnTrue($"added {newZones.Count}");
    }
}