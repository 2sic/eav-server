using System.Linq;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbApp: BllCommandBase
    {
        public DbApp(DbDataController db) : base(db, "Db.App") {}


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
        /// <param name="fullDelete">If true, the entire App is removed. Otherwise just all the contents is cleared</param>
        internal void DeleteApp(int appId, bool fullDelete)
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

                    var allJsonItemsToDelete = DbContext.SqlDb.ToSicEavEntityRelationships
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

        ///// <summary>
        ///// Deprecated code since 2020-11-09, use the other call instead. 
        ///// </summary>
        ///// <param name="appId"></param>
        //private void DeleteAppWithStoredProcedure(int appId)
        //{
        //    DbContext.SqlDb.Database.ExecuteSqlCommand("ToSIC_EAV_DeleteApp @p0", appId);
        //}

        /// <summary>
        /// Replacement for SQL call to ToSIC_EAV_DeleteApp
        /// It was developed as part of the solution to enable re-initializing an app from the xml file.
        /// In future, we'll probably never use the stored procedure any more, but we're leaving it in for now, in case something fails. 
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="alsoDeleteAppEntry"></param>
        private void DeleteAppWithoutStoredProcedure(int appId, bool alsoDeleteAppEntry)
        {
            var db = DbContext.SqlDb;
            var appContentTypes = db.ToSicEavAttributeSets.Where(a => a.AppId == appId).ToList();
            var contentTypeIds = appContentTypes.Select(ct => ct.AttributeSetId).ToArray();
            var appEntities = db.ToSicEavEntities.Where(e => appContentTypes.Contains(e.AttributeSet));
            appEntities = alsoDeleteAppEntry
                ? appEntities
                : appEntities.Where(entity => entity.ContentType != ImpExpConstants.TypeAppConfig);
            var entityIds = appEntities.Select(e => e.EntityId).ToArray();

	        //-- Delete Value-Dimensions
	        //DELETE FROM ToSIC_EAV_ValuesDimensions
	        //FROM            ToSIC_EAV_Values INNER JOIN
	        //						 ToSIC_EAV_Entities ON ToSIC_EAV_Values.EntityID = ToSIC_EAV_Entities.EntityID INNER JOIN
	        //						 ToSIC_EAV_AttributeSets ON ToSIC_EAV_Entities.AttributeSetID = ToSIC_EAV_AttributeSets.AttributeSetID INNER JOIN
	        //						 ToSIC_EAV_ValuesDimensions ON ToSIC_EAV_Values.ValueID = ToSIC_EAV_ValuesDimensions.ValueID
	        //WHERE        (ToSIC_EAV_AttributeSets.AppID = @AppID)
	        //-- Delete Values
	        //DELETE FROM ToSIC_EAV_Values
	        //FROM            ToSIC_EAV_Values INNER JOIN
	        //						 ToSIC_EAV_Entities ON ToSIC_EAV_Values.EntityID = ToSIC_EAV_Entities.EntityID INNER JOIN
	        //						 ToSIC_EAV_AttributeSets ON ToSIC_EAV_Entities.AttributeSetID = ToSIC_EAV_AttributeSets.AttributeSetID
	        //WHERE        (ToSIC_EAV_AttributeSets.AppID = @AppID)

            var appValues = db.ToSicEavValues.Where(v => entityIds.Contains(v.EntityId));
            var appValueIds = appValues.Select(a => a.ValueId).ToList();
            var valDimensions = db.ToSicEavValuesDimensions.Where(vd => appValueIds.Contains(vd.ValueId));

            //var valueDimensions = appValues
            //    .Include(av => av.ToSicEavValuesDimensions)
            //    .SelectMany(v => v.ToSicEavValuesDimensions).ToList();
            //var valueDimensions = appv
            db.RemoveRange(valDimensions);
            db.RemoveRange(appValues.ToList());


	        //-- Delete Parent-EntityRelationships
	        //DELETE FROM ToSIC_EAV_EntityRelationships
	        //FROM            ToSIC_EAV_Entities INNER JOIN
	        //						 ToSIC_EAV_AttributeSets ON ToSIC_EAV_Entities.AttributeSetID = ToSIC_EAV_AttributeSets.AttributeSetID INNER JOIN
	        //						 ToSIC_EAV_EntityRelationships ON ToSIC_EAV_Entities.EntityID = ToSIC_EAV_EntityRelationships.ParentEntityID
	        //WHERE        (ToSIC_EAV_AttributeSets.AppID = @AppID)

	        //-- Delete Child-EntityRelationships
	        //DELETE FROM ToSIC_EAV_EntityRelationships
	        //FROM            ToSIC_EAV_Entities INNER JOIN
	        //						 ToSIC_EAV_AttributeSets ON ToSIC_EAV_Entities.AttributeSetID = ToSIC_EAV_AttributeSets.AttributeSetID INNER JOIN
	        //						 ToSIC_EAV_EntityRelationships ON ToSIC_EAV_Entities.EntityID = ToSIC_EAV_EntityRelationships.ChildEntityID
	        //WHERE        (ToSIC_EAV_AttributeSets.AppID = @AppID)

            var dbRelTable = db.ToSicEavEntityRelationships;
            var relationshipsWithAppParents = dbRelTable.Where(rel => entityIds.Contains(rel.ParentEntityId));
            db.RemoveRange(relationshipsWithAppParents.ToList());
            var relationshipsWithAppChildren = dbRelTable.Where(rel => entityIds.Contains(rel.ChildEntityId ?? -1));
            db.RemoveRange(relationshipsWithAppChildren.ToList());

	        //-- Delete Entities
	        //DELETE FROM ToSIC_EAV_Entities
	        //FROM            ToSIC_EAV_Entities INNER JOIN
	        //						 ToSIC_EAV_AttributeSets ON ToSIC_EAV_Entities.AttributeSetID = ToSIC_EAV_AttributeSets.AttributeSetID
	        //WHERE        (ToSIC_EAV_AttributeSets.AppID = @AppId)
            db.RemoveRange(appEntities);

	        //-- Delete Attributes
	        //DELETE FROM ToSIC_EAV_Attributes
	        //FROM            ToSIC_EAV_Attributes INNER JOIN
	        //						 ToSIC_EAV_AttributesInSets ON ToSIC_EAV_Attributes.AttributeID = ToSIC_EAV_AttributesInSets.AttributeID INNER JOIN
	        //						 ToSIC_EAV_AttributeSets ON ToSIC_EAV_AttributesInSets.AttributeSetID = ToSIC_EAV_AttributeSets.AttributeSetID
	        //WHERE        (ToSIC_EAV_AttributeSets.AppID = @AppID)
            var attributeSetMappings =
                db.ToSicEavAttributesInSets.Where(aInS => contentTypeIds.Contains(aInS.AttributeSetId));
            var attributes = attributeSetMappings.Select(asm => asm.Attribute);
            db.RemoveRange(attributes.ToList());

	        //-- Delete Attributes not in use anywhere (Attribute not in any Set, no Values/Related Entities)
	        //DELETE FROM ToSIC_EAV_Attributes
	        //FROM            ToSIC_EAV_Attributes LEFT OUTER JOIN
	        //						 ToSIC_EAV_AttributesInSets ON ToSIC_EAV_Attributes.AttributeID = ToSIC_EAV_AttributesInSets.AttributeID LEFT OUTER JOIN
	        //						 ToSIC_EAV_EntityRelationships ON ToSIC_EAV_Attributes.AttributeID = ToSIC_EAV_EntityRelationships.AttributeID LEFT OUTER JOIN
	        //						 ToSIC_EAV_Values ON ToSIC_EAV_Attributes.AttributeID = ToSIC_EAV_Values.AttributeID
	        //WHERE        (ToSIC_EAV_Values.ValueID IS NULL) AND (ToSIC_EAV_EntityRelationships.AttributeID IS NULL) AND (ToSIC_EAV_AttributesInSets.AttributeID IS NULL)

            // note: we'll skip this for now, I don't think it's relevant...?

	        //-- Delete Attribute-In-Sets
	        //DELETE FROM ToSIC_EAV_AttributesInSets
	        //FROM            ToSIC_EAV_AttributeSets INNER JOIN
	        //						 ToSIC_EAV_AttributesInSets ON ToSIC_EAV_AttributeSets.AttributeSetID = ToSIC_EAV_AttributesInSets.AttributeSetID
	        //WHERE        (ToSIC_EAV_AttributeSets.AppID = @AppId)
            db.RemoveRange(attributeSetMappings);

	        //-- Delete AttributeSets
	        //DELETE FROM ToSIC_EAV_AttributeSets WHERE AppID = @AppId
            db.RemoveRange(appContentTypes);

	        //-- Delete App
	        //DELETE FROM ToSIC_EAV_Apps WHERE AppID = @AppId
            if (alsoDeleteAppEntry)
                db.ToSicEavApps.Remove(db.ToSicEavApps.First(a => a.AppId == appId));
        }

    }
}
