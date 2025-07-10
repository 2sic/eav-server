using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Persistence.Efc.Sys.DbContext;
using ToSic.Eav.Repository.Efc.Sys.DbEntities;
using ToSic.Eav.Serialization.Sys.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
// ReSharper disable InvokeAsExtensionMethod

namespace ToSic.Eav.Repository.Efc.Sys.DbParts;

internal class DbApp(DbStorage.DbStorage db) : DbPartBase(db, "Db.App")
{
    /// <summary>
    /// Add a new App
    /// </summary>
    internal TsDynDataApp AddApp(TsDynDataZone? zone, string guidName, int? inheritAppId = null)
    {
        // Use provided zone or if null, use the one which was pre-initialized for this DbApp Context
        zone = zone ?? DbContext.SqlDb.TsDynDataZones.SingleOrDefault(z => z.ZoneId == DbContext.ZoneId);

        var newApp = new TsDynDataApp 
        {
            Name = guidName,
            Zone = zone
        };

        // New v13 Inherited Apps - wrap in try catch because if something fails, many things could break too early for people to be able to fix
        try
        {
            if (inheritAppId > 0)
            {
                var sysSettings = new AppSysSettingsJsonInDb
                {
                    Inherit = true,
                    AncestorAppId = inheritAppId.Value
                };
                var asJson = JsonSerializer.Serialize(sysSettings, JsonOptions.SafeJsonForHtmlAttributes);
                newApp.SysSettings = asJson;
            }
        }
        catch { /* ignore */ }

        // save is required to ensure AppId is created - required for follow-up changes like EnsureSharedAttributeSets();
        DbContext.DoAndSave(() => DbContext.SqlDb.Add(newApp)); 

        return newApp;
    }


    /// <summary>
    /// Delete an existing App with any Values and Attributes
    /// </summary>
    /// <param name="appId">AppId to delete</param>
    /// <param name="fullDelete">If true, the entire App is removed. Otherwise just all the contents is cleared</param>
    internal void DeleteApp(int appId, bool fullDelete)
    {
        DbContext.Versioning.GetTransactionId();

        // Delete app
        DbContext.DoInTransaction(() =>
            {
                // Explanation: as json entities were added much later, the built-in SP to delete apps doesn't handle them correctly
                // We could update the Stored Procedure, but that's always hard to handle, so we prefer to just do it in code
                // We'll have to do the following:
                // 1. Remove relationships pointing to json-entities
                // 2. Remove json-entities
                // Between these steps, the command must be sent to the DB, so it actually allows doing the next step

                // 0. Find all json-entities, as these are the ones we treat specially
                var jsonEntitiesInAppSql = $@"SELECT e.EntityId FROM TsDynDataEntity e WHERE e.ContentTypeId = @p0 AND e.AppId = @p1";

                // If we plan to rebuild the app from the App.xml, then the config item shouldn't be deleted
                if (!fullDelete)
                    jsonEntitiesInAppSql = $@"SELECT e.EntityId FROM TsDynDataEntity e WHERE e.ContentTypeId = @p0 AND e.AppId = @p1 AND e.ContentType != @p2";

                // 1. remove all relationships to/from these json entities
                // note that actually there can only be relationships TO json entities, as all from will be in the json, 
                // but just to be sure (maybe there's historic data that's off) we'll do both
                DbContext.DoAndSave(() => ExecuteSqlCommand($@"
                    DELETE r
                    FROM TsDynDataRelationship r
                    WHERE r.ChildEntityId IN ({jsonEntitiesInAppSql})
                       OR r.ParentEntityId IN ({jsonEntitiesInAppSql})"
                    , DbEntity.RepoIdForJsonEntities, appId, AppLoadConstants.TypeAppConfig));

                // 2. remove all json entities, which won't be handled by the SP
                DbContext.DoAndSave(() => ExecuteSqlCommand($@"
                    DELETE e
                    FROM TsDynDataEntity e
                    WHERE e.EntityId IN ({jsonEntitiesInAppSql})"
                    , DbEntity.RepoIdForJsonEntities, appId, AppLoadConstants.TypeAppConfig));

                // Now let do the remaining clean-up
                DbContext.DoAndSave(() => DeleteAppWithoutStoredProcedure(appId, fullDelete));
            }
        );
    }


    /// <summary>
    /// Replacement for SQL call to ToSIC_EAV_DeleteApp
    /// It was developed as part of the solution to enable re-initializing an app from the xml file.
    /// </summary>
    /// <param name="appId"></param>
    /// <param name="alsoDeleteAppEntry"></param>
    /// <remarks>
    /// EF Core 2.1 doesn't have any built-in "DELETE WHERE…" on IQueryable
    /// RemoveRange(query) will still pull each entity into change-tracker and issue one DELETE per row.
    /// </remarks>
    private void DeleteAppWithoutStoredProcedure(int appId, bool alsoDeleteAppEntry)
    {
        // Use raw SQL for each delete to ensure server-side execution and efficiency

        // Delete Value-Dimensions
        ExecuteSqlCommand(@"
            DELETE vd
            FROM TsDynDataValueDimension vd
            INNER JOIN TsDynDataValue v ON vd.ValueId = v.ValueId
            INNER JOIN TsDynDataEntity e ON v.EntityId = e.EntityId
            WHERE e.AppId = @p0
        ", appId);

        // Delete Values
        ExecuteSqlCommand(@"
            DELETE v
            FROM TsDynDataValue v
            INNER JOIN TsDynDataEntity e ON v.EntityId = e.EntityId
            WHERE e.AppId = @p0
        ", appId);

        // Delete Parent-EntityRelationships
        ExecuteSqlCommand(@"
            DELETE r
            FROM TsDynDataRelationship r
            INNER JOIN TsDynDataEntity e ON r.ParentEntityId = e.EntityId
            WHERE e.AppId = @p0
        ", appId);

        // Delete Child-EntityRelationships
        ExecuteSqlCommand(@"
            DELETE r
            FROM TsDynDataRelationship r
            INNER JOIN TsDynDataEntity e ON r.ChildEntityId = e.EntityId
            WHERE e.AppId = @p0
        ", appId);

        // Delete Entities
        ExecuteSqlCommand(@"
            DELETE e
            FROM TsDynDataEntity e
            WHERE e.AppId = @p0
        ", appId);

        // Delete Attributes
        ExecuteSqlCommand(@"
            DELETE a
            FROM TsDynDataAttribute a
            INNER JOIN TsDynDataContentType ct ON a.ContentTypeId = ct.ContentTypeId
            WHERE ct.AppId = @p0
        ", appId);

        // Delete Content-Types
        ExecuteSqlCommand(@"
            DELETE ct
            FROM TsDynDataContentType ct
            WHERE ct.AppId = @p0
        ", appId);

        // Delete App
        if (alsoDeleteAppEntry)
            ExecuteSqlCommand(@"
                DELETE FROM TsDynDataApp WHERE AppId = @p0
            ", appId);
    }

    private void ExecuteSqlCommand(string sql, params object[] parameters)
    {
#if NETFRAMEWORK
        DbContext.SqlDb.Database.ExecuteSqlCommand(sql, parameters);
#else
        DbContext.SqlDb.Database.ExecuteSqlRaw(sql, parameters);
#endif
    }
}