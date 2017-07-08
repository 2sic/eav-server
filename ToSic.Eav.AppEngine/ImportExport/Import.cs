using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.App;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Repository.Efc;
using Entity = ToSic.Eav.Data.Entity;

namespace ToSic.Eav.Apps.ImportExport
{
    /// <summary>
    /// Import Schema and Entities to the EAV SqlStore
    /// </summary>
    public class Import
    {
        #region Private Fields
        //private readonly DbDataController _dbDeepAccess;
        private AppDataPackage _entireApp;
        
        internal AppManager App;
        internal IStorage Storage;
        public SaveOptions SaveOptions;

        private int AppId;
        private int ZoneId;

        #endregion

        /// <summary>
        /// Initializes a new instance of the Import class.
        /// </summary>
        public Import(int? zoneId, int appId, bool skipExistingAttributes = true, bool preserveUntouchedAttributes = true)
        {
            App = zoneId.HasValue ? new AppManager(zoneId.Value, appId) : new AppManager(appId);
            Storage = App.Storage;
            //_dbDeepAccess = App.DataController;// DbDataController.Instance(zoneId, appId);
            // now save the resolved zone/app IDs
            AppId = appId;
            ZoneId = App.ZoneId;// DbDeepAccess.ZoneId;
            SaveOptions = Factory.Resolve<IImportExportEnvironment>().SaveOptions(ZoneId);

            SaveOptions.SkipExistingAttributes = skipExistingAttributes;

            SaveOptions.PreserveUntouchedAttributes = preserveUntouchedAttributes;
        }

        /// <summary>
        /// Import AttributeSets and Entities
        /// </summary>
        public void ImportIntoDb(IEnumerable<ContentType> newAttributeSets, IEnumerable<Entity> newEntities)
        {
            Storage.DoWithDelayedCacheInvalidation(() =>
            {
                #region initialize DB connection / transaction

                #endregion

                // run import, but rollback transaction if necessary
                Storage.DoInTransaction(() =>
                {
                    // get initial data state for further processing, content-typed definitions etc.
                    // important: must always create a new loader, because it will cache content-types which hurts the import
                    #region import AttributeSets if any were included

                    if (newAttributeSets != null)
                        Storage.DoWhileQueuingVersioning(() =>
                        {
                            _entireApp = Storage.Loader.AppPackage(AppId); // load everything, as content-type metadata is normal entities
                            var newSetsList = newAttributeSets.ToList();
                            // first: import the attribute sets in the system scope, as they may be needed by others...
                            // ...and would need a cache-refresh before 
                            var sysAttributeSets = newSetsList.Where(a => a.Scope == Constants.ScopeSystem).ToList();
                            if (sysAttributeSets.Any())
                                MergeAndSaveContentTypes(sysAttributeSets);

                            _entireApp = Storage.Loader.AppPackage(AppId); // load everything, as content-type metadata is normal entities

                            // now the remaining attributeSets
                            var nonSysAttribSets = newSetsList.Where(a => !sysAttributeSets.Contains(a)).ToList();
                            if (nonSysAttribSets.Any())
                                MergeAndSaveContentTypes(nonSysAttribSets);
                        });

                    #endregion

                    #region import Entities

                    if (newEntities != null)
                    {
                        _entireApp = Storage.Loader.AppPackage(AppId); // load all entities
                        newEntities = newEntities
                            .Select(entity => CreateMergedForSaving(entity, _entireApp, SaveOptions))
                            .Where(e => e != null).ToList();
                        Storage.Save(newEntities.Cast<IEntity>().ToList(), SaveOptions);
                    }

                    #endregion

                });
            });
        }

        private void MergeAndSaveContentTypes(List<ContentType> contentTypes)
        {
            contentTypes.ForEach(MergeContentTypeUpdateWithExisting);
            Storage.Save(contentTypes.Cast<IContentType>().ToList(), SaveOptions.Build(ZoneId));
        }

        //private void ExtendSaveContentTypes(IEnumerable<ContentType> contentTypes)
        //    => Storage.DoWhileQueueingRelationships(() => contentTypes.ToList().ForEach(ExtendSaveContentTypes));

        ///// <summary>
        ///// Import an AttributeSet with all Attributes and AttributeMetaData
        ///// </summary>
        //private void ExtendSaveContentTypes(ContentType contentType)
        //{
        //    // initialize destinationSet - create or test existing if ok
        //    var foundSet = _dbDeepAccess.ContentType.GetOrCreateContentType(contentType);
        //    if (foundSet == null) // something went wrong, skip this import
        //        return;
        //    var contentTypeId = foundSet.Value;

        //    // append all Attributes
        //    foreach (var newAtt in contentType.Attributes.Cast<AttributeDefinition>())
        //    {
        //        var destAttribId = _dbDeepAccess.AttributesDefinition.GetOrCreateAttributeDefinition(contentTypeId, newAtt);

        //        // save additional entities containing AttributeMetaData for this attribute
        //        if (newAtt.InternalAttributeMetaData != null)
        //            SaveAttributeMetadata(destAttribId, newAtt.InternalAttributeMetaData);
        //    }

        //    // optionally re-order the attributes if specified in import
        //    if (contentType.OnSaveSortAttributes)
        //        _dbDeepAccess.ContentType.SortAttributes(contentTypeId, contentType);
        //}


        ///// <summary>
        ///// Save additional entities describing the attribute
        ///// </summary>
        ///// <param name="attributeId"></param>
        ///// <param name="metadata"></param>
        //private void SaveAttributeMetadata(int attributeId, List<Entity> metadata)
        //{
        //    var entities = new List<IEntity>();
        //    foreach (var entity in metadata)
        //    {
        //        var md = (Metadata) entity.Metadata;
        //        // Validate Entity
        //        md.TargetType = Constants.MetadataForAttribute;

        //        // Set KeyNumber
        //        if (attributeId == 0 || attributeId < 0) // < 0 is ef-core temp id
        //            throw new Exception($"trying to add metadata to attribute {attributeId} but attribute isn't saved yet");//_dbDeepAccess.SqlDb.SaveChanges();

        //        md.KeyNumber = attributeId;

        //        //// Get guid of previously existing assignment - if it exists
        //        //var existingMetadata = _dbDeepAccess.Entities
        //        //    .GetAssignedEntities(Constants.MetadataForAttribute, keyNumber: attributeId)
        //        //    .FirstOrDefault(e => e.AttributeSetId == attributeId);

        //        //if (existingMetadata != null)
        //        //    entity.SetGuid(existingMetadata.EntityGuid);

        //        //entities.Add(CreateMergedForSaving(entity, _entireApp, SaveOptions));
        //        entities.Add(entity);
        //    }
        //    Storage.Save(entities, SaveOptions.Build(ZoneId)); // don't use the standard save options, as this is attributes only
        //}
        


        private void MergeContentTypeUpdateWithExisting(IContentType contentType)
        {
            var existing = _entireApp.ContentTypes.Values.FirstOrDefault(ct => ct.StaticName == contentType.StaticName);
            if (existing == null) return;

            foreach (var newAttrib in contentType.Attributes)
            {
                var existAttrib = existing.Attributes.FirstOrDefault(a => a.Name == newAttrib.Name);
                if (existAttrib == null) continue;

                var impMeta = ((AttributeDefinition) newAttrib).Items;
                var newMetaList = new List<IEntity>();
                foreach (var newMd in impMeta)
                {
                    var existingMetadata = _entireApp.GetMetadata(Constants.MetadataForAttribute, existAttrib.AttributeId, newMd.Type.StaticName).FirstOrDefault();
                    if (existingMetadata == null)
                        newMetaList.Add(newMd);
                    else
                        newMetaList.Add(EntitySaver.CreateMergedForSaving(existingMetadata, newMd, SaveOptions) as Entity);
                }
                ((AttributeDefinition) newAttrib).AddItems(newMetaList);
            }
        }

        /// <summary>
        /// Import an Entity with all values
        /// </summary>
        private Entity CreateMergedForSaving(Entity update, AppDataPackage appDataPackage, SaveOptions saveOptions)
        {
           #region try to get AttributeSet or otherwise cancel & log error

            var dbAttrSet = appDataPackage.ContentTypes.Values
                .FirstOrDefault(ct => String.Equals(ct.StaticName, update.Type.StaticName, StringComparison.InvariantCultureIgnoreCase));

            if (dbAttrSet == null) // AttributeSet not Found
            {
                Storage.Log.Add(new LogItem(EventLogEntryType.Error, "ContentType not found for " + update.Type.StaticName));
                return null;
            }

            #endregion

            // Find existing Enties - meaning both draft and non-draft
            List<IEntity> existingEntities = null;
            if (update.EntityGuid != Guid.Empty)
                existingEntities = appDataPackage.List.Where(e => e.EntityGuid == update.EntityGuid).ToList();

            #region Simplest case - nothing existing to update: return entity

            if (existingEntities == null || !existingEntities.Any())
                return update;

            #endregion

            Storage.Log.Add(new LogItem(EventLogEntryType.Information, $"FYI: Entity {update.EntityId} already exists for guid {update.EntityGuid}"));

            // now update (main) entity id from existing - since it already exists
            var original = existingEntities.First();
            update.ChangeIdForSaving(original.EntityId);
            return EntitySaver.CreateMergedForSaving(original, update, saveOptions) as Entity;

        }
    }
}