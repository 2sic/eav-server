using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbZone: BllCommandBase
    {
        public DbZone(DbDataController cntx) : base(cntx, "Db.Zone") {}
       
        /// <summary>
        /// Creates a new Zone with a default App and Culture-Root-Dimension
        /// </summary>
        public int AddZone(string name)
        {
            var newZone = new ToSicEavZones { Name = name };
            DbContext.SqlDb.Add(newZone);

            DbContext.Dimensions.AddRootCultureNode(Constants.CultureSystemKey, "Culture Root", newZone);

            DbContext.App.AddApp(newZone, Constants.DefaultAppGuid);

            // TODO: #AppState v13 - activate this once
            // We reliably auto-init the site-app by default
            DbContext.App.AddApp(newZone, Constants.PrimaryAppGuid);

            return newZone.ZoneId;
        }



        public bool AddMissingPrimaryApps()
        {
            var wrapLog = Log.Call<bool>();

            var zonesWithoutPrimary = DbContext.SqlDb.ToSicEavZones
                .Include(z => z.ToSicEavApps)
                .Include(z => z.ToSicEavDimensions)
                // Skip "default" zone as that is a single purpose technical zone
                .Where(z => z.ZoneId != Constants.DefaultZoneId)
                .Where(z => z.ToSicEavApps.All(a => a.Name != Constants.PrimaryAppGuid))
                .ToList();

            if (!zonesWithoutPrimary.Any()) return wrapLog("no missing primary", false);

            var newZones = zonesWithoutPrimary
                .Select(zone => new ToSicEavApps
                {
                    Name = Constants.PrimaryAppGuid,
                    Zone = zone
                }).ToList();

            DbContext.DoAndSave(() => DbContext.SqlDb.AddRange(newZones));

            return wrapLog($"added {newZones.Count}", true);
        }
    }
}
