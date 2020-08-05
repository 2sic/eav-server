using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Persistence.Efc
{
    public partial class Efc11Loader
    {
        public IReadOnlyDictionary<int, Zone> Zones()
        {
            var log = new Log("DB.EfLoad", null, "Zones()");
            History.Add("zone-app-map", log);

            var zones = _dbContext.ToSicEavZones
                .Include(z => z.ToSicEavApps)
                .Include(z => z.ToSicEavDimensions)
                .ThenInclude(d => d.ParentNavigation)
                .ToDictionary(z => z.ZoneId, z => new Zone(z.ZoneId,
                    z.ToSicEavApps.FirstOrDefault(a => a.Name == Constants.DefaultAppName)?.AppId ?? -1,
                    z.ToSicEavApps.ToDictionary(a => a.AppId, a => a.Name),
                    z.ToSicEavDimensions.Where(d => d.ParentNavigation?.Key == Constants.CultureSystemKey)
                        // ReSharper disable once RedundantEnumerableCastCall
                        .Cast<DimensionDefinition>().ToList()));

            return zones;
        }
    }
}
