using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbApp: BllCommandBase
    {
        public DbApp(DbDataController cntx) : base(cntx, "Db.App") {}


        /// <summary>
        /// Add a new App
        /// </summary>
        internal ToSicEavApps AddApp(ToSicEavZones zone, string name = Constants.DefaultAppName)
        {
            zone = zone ?? DbContext.SqlDb.ToSicEavZones.SingleOrDefault(z => z.ZoneId == DbContext.ZoneId);

            var newApp = new ToSicEavApps 
            {
                Name = name,
                Zone = zone
            };
            DbContext.DoAndSave(() => DbContext.SqlDb.Add(newApp)); // save is required to ensure AppId is created - required in EnsureSharedAttributeSets();

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
            DbContext.DoInTransaction(() =>
                {
                    // Explanation: as json entities were added much later, the built-in SP to delete apps doesn't handle them correctly
                    // We could update the Stored Procedure, but that's always hard to handle, so we prefer to just do it in code
                    // We'll have to do the following:
                    // 1. Remove relationships pointing to json-entities
                    // 2. Remove json-entities
                    // Between these steps, the command must be sent to the DB, so it actually allows doing the next step

                    // 0. Find all json-entities, as these are the ones we treat specially
                    var jsonEntitiesInApp = DbContext.SqlDb.ToSicEavEntities
                        .Where(e => e.AttributeSetId == DbEntity.RepoIdForJsonEntities
                                    && e.AppId == appId);

                    // 1. remove all relationships to/from these json entities
                    // note that actually there can only be relationships TO json entities, as all from will be in the json, 
                    // but just to be sure (maybe there's historic data that's off) we'll do both
                    DbContext.DoAndSave(() =>
                        DbContext.SqlDb.RemoveRange(
                            DbContext.SqlDb.ToSicEavEntityRelationships.Where(r =>
                                jsonEntitiesInApp.Any(e =>
                                    e.EntityId == r.ChildEntityId || e.EntityId == r.ParentEntityId)
                            ))
                    );

                    // 2. remove all json entities, which won't be handled by the SP
                    DbContext.DoAndSave(() => DbContext.SqlDb.RemoveRange(jsonEntitiesInApp));

                    // Now let the Stored Procedure do the remaining clean-up
                    DbContext.SqlDb.Database.ExecuteSqlCommand("ToSIC_EAV_DeleteApp @p0", appId);
                }
            );
        }

    }
}
