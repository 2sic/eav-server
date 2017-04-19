using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Persistence.EFC11.Models;

namespace ToSic.Eav.Persistence.EFC11.Repository
{
    public class AppRepo: EfcRepoPart
    {
        public AppRepo(EfcRepository parent) : base(parent) { }


        /// <summary>
        /// Add a new App
        /// </summary>
        public ToSicEavApps AddApp(ToSicEavZones zone, string name = Constants.DefaultAppName)
        {
            if (zone == null)
                zone = Db.ToSicEavZones.SingleOrDefault(z => z.ZoneId == Parent.ZoneId); // GetZone(Parent.ZoneId);

            var newApp = new ToSicEavApps()
            {
                Name = name,
                Zone = zone
            };
            Db.ToSicEavApps.Add(newApp);

            Db.SaveChanges();	// required to ensure AppId is created - required in EnsureSharedAttributeSets();

            DbContext.AttribSet.EnsureSharedAttributeSets(newApp);

            // 2017-04-01 removed, shouldn't be necessary any more at this level
            // PurgeGlobalCache(Context.ZoneId, Context.AppId);

            return newApp;
        }


    }
}
