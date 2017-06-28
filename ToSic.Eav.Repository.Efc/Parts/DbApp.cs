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
            zone = zone ?? DbContext.SqlDb.ToSicEavZones.SingleOrDefault(z => z.ZoneId == DbContext.ZoneId);

            var newApp = new ToSicEavApps 
            {
                Name = name,
                Zone = zone
            };
            DbContext.DoAndSave(() => DbContext.SqlDb.Add(newApp)); // save is required to ensure AppId is created - required in EnsureSharedAttributeSets();
            DbContext.DoAndSave(() => DbContext.AttribSet.PrepareMissingSharedAttributesOnApp(newApp));

            return newApp;
        }


        /// <summary>
        /// Delete an existing App with any Values and Attributes
        /// </summary>
        /// <param name="appId">AppId to delete</param>
        internal void DeleteApp(int appId)
        {
            DbContext.Versioning.GetChangeLogId();

            // Delete app using StoredProcedure
            DbContext.SqlDb.Database.ExecuteSqlCommand("ToSIC_EAV_DeleteApp @p0", appId);
        }

    }
}
