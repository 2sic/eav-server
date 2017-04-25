using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbApp: BllCommandBase
    {
        public DbApp(DbDataController cntx) : base(cntx) {}


        /// <summary>
        /// Add a new App
        /// </summary>
        public ToSicEavApps AddApp(ToSicEavZones zone, string name = Constants.DefaultAppName)
        {
            if (zone == null)
                zone = DbContext.Zone.GetZone(DbContext.ZoneId);

            var newApp = new ToSicEavApps
            {
                Name = name,
                Zone = zone
            };
            DbContext.SqlDb.Add(newApp);

            DbContext.SqlDb.SaveChanges();	// required to ensure AppId is created - required in EnsureSharedAttributeSets();

            DbContext.AttribSet.EnsureSharedAttributeSets(newApp);

            // 2017-04-01 removed, shouldn't be necessary any more at this level
            // PurgeGlobalCache(Context.ZoneId, Context.AppId);

            return newApp;
        }

        // 2017-04-01 2dm remove from this layer...
        //private void PurgeGlobalCache(int zoneId, int appId)
        //{
        //    // todo: bad - don't want any data-source in here!
        //    DataSource.GetCache(zoneId, appId).PurgeGlobalCache();
        //}

        ///// <summary>
        ///// Add a new App to the current Zone
        ///// </summary>
        ///// <param name="name">The name of the new App</param>
        ///// <returns></returns>
        //public App AddApp(string name)
        //    => AddApp(Context.Zone.GetZone(Context.ZoneId), name);


        /// <summary>
        /// Delete an existing App with any Values and Attributes
        /// </summary>
        /// <param name="appId">AppId to delete</param>
        public void DeleteApp(int appId)
        {
            // 2017-04-06 2dm disabled this check, as of now the context doesn't know the app-id as it's comming from the Zone
            //if (appId != DbContext.AppId)  // this only happens if there is some kind of id-fallback
            //    throw new Exception("An app can only be removed inside of it's own context.");

            // enure changelog exists and is set to SQL CONTEXT_INFO variable
            if (DbContext.Versioning.MainChangeLogId == 0)
                DbContext.Versioning.GetChangeLogId(DbContext.UserName);

            // Delete app using StoredProcedure
            DbContext.SqlDb.Database.ExecuteSqlCommand("ToSIC_EAV_DeleteApp @p0", appId);// .DeleteAppInternal(appId);

            // 2017-04-01 2dm removed from here, must happen at "outer" layer
            // Remove App from Global Cache
            // PurgeGlobalCache(Context.ZoneId, Context.AppId);
        }

        /// <summary>
        /// Get all Apps in the current Zone
        /// </summary>
        /// <returns></returns>
        public List<ToSicEavApps> GetApps()
            => DbContext.SqlDb.ToSicEavApps.Where(a => a.ZoneId == DbContext.ZoneId).ToList();
        

    }
}
