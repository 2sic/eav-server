using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader
    {

        public IDictionary<int, Zone> Zones()
        {
            var log = new Log("DB.EfLoad", null, "Zones()");
            _logHistory.Add("zone-app-map", log);

            // Build the tree of zones incl. their default(Content) and Primary apps
            var zones = _dbContext.ToSicEavZones
                .Include(z => z.ToSicEavApps)
                .Include(z => z.ToSicEavDimensions)
                .ThenInclude(d => d.ParentNavigation)
                .ToDictionary(
                    z => z.ZoneId,
                    z =>
                    {
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
            return zones;
        }

        //private bool AddMissingPrimaryApps()
        //{
        //    var wrapLog = Log.Call<bool>();

        //    var zonesWithoutPrimary = _dbContext.ToSicEavZones
        //        .Include(z => z.ToSicEavApps)
        //        .Include(z => z.ToSicEavDimensions)
        //        // Skip "default" zone as that is a single purpose technical zone
        //        .Where(z => z.ZoneId != Constants.DefaultZoneId)
        //        .Where(z => z.ToSicEavApps.All(a => a.Name != Constants.PrimaryAppGuid))
        //        .ToList();

        //    if (!zonesWithoutPrimary.Any()) return wrapLog("no missing primary", false);

        //    var newZones = zonesWithoutPrimary
        //        .Select(zone => new ToSicEavApps
        //        {
        //            Name = Constants.PrimaryAppGuid, 
        //            Zone = zone
        //        }).ToList();

        //    _dbContext.AddRange(newZones);
        //    _dbContext.SaveChanges();

        //    return wrapLog($"added {newZones.Count}", true);
        //}
    }
}
