using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Sys;

namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

internal class DbZone(DbStorage.DbStorage db) : DbPartBase(db, "Db.Zone")
{
    /// <summary>
    /// Creates a new Zone with a default App and Culture-Root-Dimension
    /// </summary>
    public int AddZone(int tenantId, int siteId, string name, string appBasePath, string appBaseSharedPath)
    {
        var newZone = new TsDynDataZone
        {
            Name = name,
            TenantId = tenantId,
            SiteId = siteId,
            AppBasePath = appBasePath,
            AppBaseSharedPath = appBaseSharedPath
        };
        DbStore.SqlDb.Add(newZone);

        DbStore.Dimensions.AddRootCultureNode(EavConstants.CultureSystemKey, "Culture Root", newZone);

        DbStore.App.AddAppAndSave(newZone.ZoneId, KnownAppsConstants.DefaultAppGuid);

        // We reliably auto-init the site-app by default
        DbStore.App.AddAppAndSave(newZone.ZoneId, KnownAppsConstants.PrimaryAppGuid);

        return newZone.ZoneId;
    }



    public bool AddMissingPrimaryApps()
    {
        var l = LogSummary.Fn<bool>();

        var zonesWithoutPrimary = DbStore.SqlDb.TsDynDataZones
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

        DbStore.DoAndSaveWithoutChangeDetection(() => DbStore.SqlDb.AddRange(newZones));

        return l.ReturnTrue($"added {newZones.Count}");
    }
}