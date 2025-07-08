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
                var jsonEntitiesInApp = DbContext.SqlDb.TsDynDataEntities
                    .Where(e => e.ContentTypeId == DbEntity.RepoIdForJsonEntities
                                && e.AppId == appId);
                    
                // If we plan to rebuild the app from the App.xml, then the config item shouldn't be deleted
                if (!fullDelete)
                    jsonEntitiesInApp =
                        jsonEntitiesInApp.Where(entity => entity.ContentType != AppLoadConstants.TypeAppConfig);

                // 1. remove all relationships to/from these json entities
                // note that actually there can only be relationships TO json entities, as all from will be in the json, 
                // but just to be sure (maybe there's historic data that's off) we'll do both

                var allJsonItemsToDelete = DbContext.SqlDb.TsDynDataRelationships
                    .Where(r => 
                        jsonEntitiesInApp.Any(e => e.EntityId == r.ChildEntityId || e.EntityId == r.ParentEntityId));
                DbContext.DoAndSave(() => DbContext.SqlDb.RemoveRange(allJsonItemsToDelete));

                // 2. remove all json entities, which won't be handled by the SP
                DbContext.DoAndSave(() => DbContext.SqlDb.RemoveRange(jsonEntitiesInApp));

                // Now let the Stored Procedure do the remaining clean-up
                //DeleteAppWithStoredProcedure(appId);
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
        var db = DbContext.SqlDb;
        var appContentTypes = db.TsDynDataContentTypes
            .Where(a => a.AppId == appId);
        
        // WIP v13 - now with Inherited Apps, we have entities which point to a content-type which doesn't belong to the App itself
        const bool useV12Method = false;
        var appEntities = useV12Method
            ? db.TsDynDataEntities.Join(appContentTypes, e => e.ContentTypeId, ct => ct.ContentTypeId, (e, ct) => e)
            : db.TsDynDataEntities.Where(e => e.AppId == appId);

        // Delete Value-Dimensions
        var appValues = db.TsDynDataValues.Join(
            appEntities,
            v => v.EntityId,
            e => e.EntityId,
            (v, e) => v);
        var valDimensions = db.TsDynDataValueDimensions.Join(
            appValues,
            vd => vd.ValueId,
            v => v.ValueId,
            (vd, v) => vd);
        db.RemoveRange(valDimensions);
        db.RemoveRange(appValues);

        // Delete Parent-EntityRelationships & Child-EntityRelationships
        var dbRelTable = db.TsDynDataRelationships;
        var relationshipsWithAppParents = dbRelTable.Join(
            appEntities,
            rel => rel.ParentEntityId,
            e => e.EntityId,
            (rel, e) => rel);
        db.RemoveRange(relationshipsWithAppParents);
        var relationshipsWithAppChildren = dbRelTable.Join(
            appEntities,
            rel => rel.ChildEntityId,
            e => e.EntityId,
            (rel, e) => rel);
        db.RemoveRange(relationshipsWithAppChildren);

        // Delete Entities
        db.RemoveRange(appEntities);

        // Delete Attributes
        var attributes = db.TsDynDataAttributes.Join(
            appContentTypes,
            a => a.ContentTypeId,
            ct => ct.ContentTypeId,
            (a, ct) => a);
        db.RemoveRange(attributes);

        // Delete Content-Types
        db.RemoveRange(appContentTypes);

        // Delete App
        if (alsoDeleteAppEntry)
            db.TsDynDataApps.Remove(db.TsDynDataApps.First(a => a.AppId == appId));
    }

}