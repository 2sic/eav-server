using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Repository.EF4.Parts
{
    public class DbZone: BllCommandBase
    {
        public DbZone(DbDataController cntx) : base(cntx) {}


        /// <summary>
        /// Get all Zones
        /// </summary>
        /// <returns>Dictionary with ZoneId as Key and ZoneModel</returns>
        public Dictionary<int, Data.Zone> GetAllZones()
            => DbContext.SqlDb.Zones.ToDictionary(z => z.ZoneID, z => new Data.Zone(
                z.ZoneID,
                z.Apps.FirstOrDefault(a => a.Name == Constants.DefaultAppName).AppID,
                z.Apps.ToDictionary(a => a.AppID, a => a.Name)));
            

        /// <summary>
        /// Get all Zones
        /// </summary>
        /// <returns></returns>
        public List<Zone> GetZones()
            => DbContext.SqlDb.Zones.ToList();


        /// <summary>
        /// Get a single Zone
        /// </summary>
        /// <returns>Zone or null</returns>
        public Zone GetZone(int zoneId)
            => DbContext.SqlDb.Zones.SingleOrDefault(z => z.ZoneID == zoneId);
        



        /// <summary>
        /// Creates a new Zone with a default App and Culture-Root-Dimension
        /// </summary>
        public Tuple<Zone, App> AddZone(string name)
        {
            var newZone = new Zone { Name = name };
            DbContext.SqlDb.AddToZones(newZone);

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
        //    var zone = Context.SqlDb.Zones.Single(z => z.ZoneID == zoneId);
        //    zone.Name = name;
        //    Context.SqlDb.SaveChanges();
        //}
    }
}
