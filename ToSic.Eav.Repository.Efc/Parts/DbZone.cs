using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbZone: BllCommandBase
    {
        public DbZone(DbDataController cntx) : base(cntx) {}


        /// <summary>
        /// Get all Zones
        /// </summary>
        /// <returns>Dictionary with ZoneId as Key and ZoneModel</returns>
        public Dictionary<int, Data.Zone> GetAllZones()
            => DbContext.SqlDb.ToSicEavZones.ToDictionary(z => z.ZoneId, z => new Data.Zone(
                z.ZoneId,
                z.ToSicEavApps.FirstOrDefault(a => a.Name == Constants.DefaultAppName).AppId,
                z.ToSicEavApps.ToDictionary(a => a.AppId, a => a.Name)));
            

        /// <summary>
        /// Get all Zones
        /// </summary>
        /// <returns></returns>
        public List<ToSicEavZones> GetZones()
            => DbContext.SqlDb.ToSicEavZones.ToList();


        /// <summary>
        /// Get a single Zone
        /// </summary>
        /// <returns>Zone or null</returns>
        public ToSicEavZones GetZone(int zoneId)
            => DbContext.SqlDb.ToSicEavZones.SingleOrDefault(z => z.ZoneId == zoneId);
        



        /// <summary>
        /// Creates a new Zone with a default App and Culture-Root-Dimension
        /// </summary>
        public Tuple<ToSicEavZones, ToSicEavApps> AddZone(string name)
        {
            var newZone = new ToSicEavZones { Name = name };
            DbContext.SqlDb.Add(newZone);

            DbContext.Dimensions.AddDimension(Constants.CultureSystemKey, "Culture Root", newZone);

            var newApp = DbContext.App.AddApp(newZone);

            DbContext.SqlDb.SaveChanges();

            return Tuple.Create(newZone, newApp);
        }

        // 2017-04-05
        ///// <summary>
        ///// Update a Zone
        ///// </summary>
        //public void UpdateZone(int zoneId, string name)
        //{
        //    var zone = Context.SqlDb.Zones.Single(z => z.ZoneId == zoneId);
        //    zone.Name = name;
        //    Context.SqlDb.SaveChanges();
        //}
    }
}
