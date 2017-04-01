using System;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.BLL.Parts
{
    public class DbApp: BllCommandBase
    {
        public DbApp(EavDataController cntx) : base(cntx) {}


        /// <summary>
        /// Add a new App
        /// </summary>
        public App AddApp(Zone zone, string name = Constants.DefaultAppName)
        {
            if (zone == null)
                zone = Context.Zone.GetZone(Context.ZoneId);

            var newApp = new App
            {
                Name = name,
                Zone = zone
            };
            Context.SqlDb.AddToApps(newApp);

            Context.SqlDb.SaveChanges();	// required to ensure AppId is created - required in EnsureSharedAttributeSets();

            Context.AttribSet.EnsureSharedAttributeSets(newApp);

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
            if (appId != Context.AppId)  // this only happens if there is some kind of id-fallback
                throw new Exception("An app can only be removed inside of it's own context.");

            // enure changelog exists and is set to SQL CONTEXT_INFO variable
            if (Context.Versioning.MainChangeLogId == 0)
                Context.Versioning.GetChangeLogId(Context.UserName);

            // Delete app using StoredProcedure
            Context.SqlDb.DeleteAppInternal(appId);

            // 2017-04-01 2dm removed from here, must happen at "outer" layer
            // Remove App from Global Cache
            // PurgeGlobalCache(Context.ZoneId, Context.AppId);
        }

        /// <summary>
        /// Get all Apps in the current Zone
        /// </summary>
        /// <returns></returns>
        public List<App> GetApps()
            => Context.SqlDb.Apps.Where(a => a.ZoneID == Context.ZoneId).ToList();
        

    }
}
