using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.BLL;
using ToSic.Eav.DataSources.Caches;

namespace ToSic.Eav.Import
{
    /// <summary>
    /// Import Schema and Entities to the EAV SqlStore
    /// </summary>
    public class Import
    {
        #region Private Fields
        private readonly EavDataController Context;
        private readonly bool _dontUpdateExistingAttributeValues;
        private readonly bool _keepAttributesMissingInImport;
        private readonly List<ImportLogItem> _importLog = new List<ImportLogItem>();
        private readonly bool LargeImport;
        #endregion

        #region Properties
        /// <summary>
        /// Get the Import Log
        /// </summary>
        public List<ImportLogItem> ImportLog
        {
            get { return _importLog; }
        }

        bool PreventUpdateOnDraftEntities { get; set; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the Import class.
        /// </summary>
        public Import(int? zoneId, int? appId, string userName, bool dontUpdateExistingAttributeValues = true, bool keepAttributesMissingInImport = true, bool preventUpdateOnDraftEntities = true, bool largeImport = true)
        {
            Context = EavDataController.Instance(zoneId, appId);

            Context.UserName = userName;
            _dontUpdateExistingAttributeValues = dontUpdateExistingAttributeValues;
            _keepAttributesMissingInImport = keepAttributesMissingInImport;
            PreventUpdateOnDraftEntities = preventUpdateOnDraftEntities;
            LargeImport = largeImport;
        }

        /// <summary>
        /// Import AttributeSets and Entities
        /// </summary>
        public DbTransaction RunImport(IEnumerable<ImportAttributeSet> newAttributeSets, IEnumerable<ImportEntity> newEntities)
        {
            // 2dm 2015-06-21: this doesn't seem to be used anywhere else in the entire code!
            Context.PurgeAppCacheOnSave = false;
            var commitTransaction = true;
            bool purgeCache = true;

            // Enhance the SQL timeout for imports
            // todo 2dm/2tk - discuss, this shouldn't be this high on a normal save, only on a real import
            // todo: on any error, cancel/rollback the transaction
            if (LargeImport)
                Context.SqlDb.CommandTimeout = 3600;

            // Ensure cache is created
            var y = DataSource.GetCache(Constants.DefaultZoneId, Constants.MetaDataAppId).LightList.Any();
            var cache = DataSource.GetCache(Context.ZoneId, Context.AppId);
            cache.PurgeCache(Context.ZoneId, Context.AppId);
            var x = cache.LightList.Any(); // re-read something

            #region initialize DB connection / transaction
            // Make sure the connection is open - because on multiple calls it's not clear if it was already opened or not
            if (Context.SqlDb.Connection.State != ConnectionState.Open)
                Context.SqlDb.Connection.Open();

            var transaction = Context.SqlDb.Connection.BeginTransaction();

            #endregion

            try // run import, but rollback transaction if necessary
            {

                #region import AttributeSets if any were included

                if (newAttributeSets != null)
                {
                    // first: import the attribute sets in the system scope, as they may be needed by others...
                    // ...and would need a cache-refresh before 
                    var sysAttributeSets = newAttributeSets.Where(a => a.Scope == Constants.ScopeSystem);
                    if(sysAttributeSets.Any())
                        transaction = ImportSomeAttributeSets(sysAttributeSets, transaction);

                    // now the remaining attributeSets
                    var nonSysAttribSets = newAttributeSets.Where(a => !sysAttributeSets.Contains(a));
                    if(nonSysAttribSets.Any())
                        transaction = ImportSomeAttributeSets(nonSysAttribSets, transaction);
                }

                #endregion

                #region import Entities
                if (newEntities != null)
                {
                    foreach (var entity in newEntities)
                        PersistOneImportEntity(entity);

                    Context.Relationships.ImportEntityRelationshipsQueue();

                    Context.SqlDb.SaveChanges();
                }
                #endregion

                // Commit DB Transaction
                transaction.Commit();
                Context.SqlDb.Connection.Close();

                // Purge Cache
                if (purgeCache)
                    DataSource.GetCache(Context.ZoneId, Context.AppId).PurgeCache(Context.ZoneId, Context.AppId);

                return transaction;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }

        private DbTransaction ImportSomeAttributeSets(IEnumerable<ImportAttributeSet> newAttributeSets, DbTransaction transaction)
        {
            bool y;
            ICache cache;
            bool x;
            foreach (var attributeSet in newAttributeSets)
                ImportAttributeSet(attributeSet);

            Context.Relationships.ImportEntityRelationshipsQueue();

            Context.AttribSet.EnsureSharedAttributeSets();

            Context.SqlDb.SaveChanges();

            // Commit DB Transaction and refresh cache
            transaction.Commit();
            y = DataSource.GetCache(Constants.DefaultZoneId, Constants.MetaDataAppId).LightList.Any();
            cache = DataSource.GetCache(Context.ZoneId, Context.AppId);
            cache.PurgeCache(Context.ZoneId, Context.AppId);
            x = cache.LightList.Any(); // re-read something

            // re-start transaction
            transaction = Context.SqlDb.Connection.BeginTransaction();
            return transaction;
        }

        /// <summary>
        /// Import an AttributeSet with all Attributes and AttributeMetaData
        /// </summary>
        private void ImportAttributeSet(ImportAttributeSet importAttributeSet)
        {
            var destinationSet = Context.AttribSet.GetAttributeSet(importAttributeSet.StaticName);
            // add new AttributeSet
            if (destinationSet == null)
                destinationSet = Context.AttribSet.AddAttributeSet(importAttributeSet.Name, importAttributeSet.Description, importAttributeSet.StaticName, importAttributeSet.Scope, false);
            else	// use/update existing attribute Set
            {
                if (destinationSet.UsesConfigurationOfAttributeSet.HasValue)
                {
                    _importLog.Add(new ImportLogItem(EventLogEntryType.Error, "Not allowed to import/extend an AttributeSet which uses Configuration of another AttributeSet.") { ImportAttributeSet = importAttributeSet });
                    return;
                }

                _importLog.Add(new ImportLogItem(EventLogEntryType.Information, "AttributeSet already exists") { ImportAttributeSet = importAttributeSet });
            }

	        destinationSet.AlwaysShareConfiguration = importAttributeSet.AlwaysShareConfiguration;

            // If a "Ghost"-content type is specified, try to assign that
            if (!string.IsNullOrEmpty(importAttributeSet.UsesConfigurationOfAttributeSet))
            {
                // Look for a content type with the StaticName, which has no "UsesConfigurationOf..." set (is a master)
                var ghostAttributeSets = Context.SqlDb.AttributeSets.Where(a => a.StaticName == importAttributeSet.UsesConfigurationOfAttributeSet && a.ChangeLogDeleted == null && a.UsesConfigurationOfAttributeSet == null).
                    OrderBy(a => a.AttributeSetID).ToList();

                if (ghostAttributeSets.Count == 0)
                {
                    _importLog.Add(new ImportLogItem(EventLogEntryType.Warning, "AttributeSet not imported because master set not found: " + importAttributeSet.UsesConfigurationOfAttributeSet) { ImportAttributeSet = importAttributeSet });
                    return;
                }

                // If multiple masters are found, use first and add a warning message
                if (ghostAttributeSets.Count > 1)
                    _importLog.Add(new ImportLogItem(EventLogEntryType.Warning, "Multiple potential master AttributeSets found for StaticName: " + importAttributeSet.UsesConfigurationOfAttributeSet) { ImportAttributeSet = importAttributeSet });
                destinationSet.UsesConfigurationOfAttributeSet = ghostAttributeSets.First().AttributeSetID;
            }
            

            if (destinationSet.AlwaysShareConfiguration)
	        {
		        Context.AttribSet.EnsureSharedAttributeSets();
	        }
	        Context.SqlDb.SaveChanges();

            // append all Attributes
            foreach (var importAttribute in importAttributeSet.Attributes)
            {
                Eav.Attribute destinationAttribute;
                var isNewAttribute = false;
                try	// try to add new Attribute
                {
                    var isTitle = importAttribute == importAttributeSet.TitleAttribute;
                    destinationAttribute = Context.Attributes.AppendAttribute(destinationSet, importAttribute.StaticName, importAttribute.Type, importAttribute.InputType, isTitle, false);
                    isNewAttribute = true;
                }
				catch (ArgumentException ex)	// Attribute already exists
                {
					_importLog.Add(new ImportLogItem(EventLogEntryType.Warning, "Attribute already exists") { ImportAttribute = importAttribute, Exception = ex });
                    destinationAttribute = destinationSet.AttributesInSets.Single(a => a.Attribute.StaticName == importAttribute.StaticName).Attribute;
                }

                var mds = DataSource.GetMetaDataSource(Context.ZoneId, Context.AppId);
                // Insert AttributeMetaData
                if (/* isNewAttribute && */ importAttribute.AttributeMetaData != null)
                {
                    foreach (var entity in importAttribute.AttributeMetaData)
                    {
                        // Validate Entity
                        entity.AssignmentObjectTypeId = Constants.AssignmentObjectTypeIdFieldProperties;

                        // Set KeyNumber
                        if (destinationAttribute.AttributeID == 0)
                            Context.SqlDb.SaveChanges();
                        entity.KeyNumber = destinationAttribute.AttributeID;

                        var existingEntity = mds.GetAssignedEntities(Constants.AssignmentObjectTypeIdFieldProperties,
                            destinationAttribute.AttributeID, entity.AttributeSetStaticName).FirstOrDefault();

                        if (existingEntity != null)
                            entity.EntityGuid = existingEntity.EntityGuid;

                        PersistOneImportEntity(entity);
                    }
                }
            }
        }

        /// <summary>
        /// Import an Entity with all values
        /// </summary>
        private void PersistOneImportEntity(ImportEntity importEntity)
        {
            var cache = DataSource.GetCache(null, Context.AppId);

            #region try to get AttributeSet or otherwise cancel & log error

            // var attributeSet = Context.AttribSet.GetAttributeSet(importEntity.AttributeSetStaticName);
            var attributeSet = cache.GetContentType(importEntity.AttributeSetStaticName);
            if (attributeSet == null) // AttributeSet not Found
            {
                _importLog.Add(new ImportLogItem(EventLogEntryType.Error, "AttributeSet not found")
                {
                    ImportEntity = importEntity,
                    ImportAttributeSet = new ImportAttributeSet {StaticName = importEntity.AttributeSetStaticName}
                });
                return;
            }

            #endregion

            // Find existing Enties - meaning both draft and non-draft
            List<IEntity> existingEntities = null;
            if (importEntity.EntityGuid.HasValue)
                existingEntities = cache.LightList.Where(e => e.EntityGuid == importEntity.EntityGuid.Value).ToList();

            #region Simplest case - add (nothing existing to update)
            if (existingEntities == null || !existingEntities.Any())
            {
                Context.Entities.AddEntity(attributeSet.AttributeSetId, importEntity, _importLog, importEntity.IsPublished, null);
                return;
            }

            #endregion

            // todo: 2dm 2016-06-29 check this
            #region Another simple case - we have published entities, but are saving unpublished - so we create a new one

            if (!importEntity.IsPublished && existingEntities.Count(e => e.IsPublished == false) == 0 && !importEntity.ForceNoBranch)
            {
                var publishedId = existingEntities.First().EntityId;
                Context.Entities.AddEntity(attributeSet.AttributeSetId, importEntity, _importLog, importEntity.IsPublished, publishedId);
                return;
            }

            #endregion 
             
            #region Update-Scenario - much more complex to decide what to change/update etc.

            #region Do Various Error checking like: Does it really exist, is it not draft, ensure we have the correct Content-Type

            // Get existing, published Entity
            var editableVersionOfTheEntity = existingEntities.OrderBy(e => e.IsPublished ? 1 : 0).First(); // get draft first, otherwise the published
            _importLog.Add(new ImportLogItem(EventLogEntryType.Information, "Entity already exists", importEntity));
        

            #region ensure we don't save a draft is this is not allowed (usually in the case of xml-import)

            // Prevent updating Draft-Entity - since the initial would be draft if it has one, this would throw
            if (PreventUpdateOnDraftEntities && !editableVersionOfTheEntity.IsPublished)
            {
                _importLog.Add(new ImportLogItem(EventLogEntryType.Error, "Importing a Draft-Entity is not allowed", importEntity));
                return;
            }

            #endregion

            #region Ensure entity has same AttributeSet (do this after checking for the draft etc.
            if (editableVersionOfTheEntity.Type.StaticName != importEntity.AttributeSetStaticName)
            {
                _importLog.Add(new ImportLogItem(EventLogEntryType.Error, "Existing entity (which should be updated) has different ContentType", importEntity));
                return;
            }
            #endregion



            #endregion

            var newValues = importEntity.Values;
            if (_dontUpdateExistingAttributeValues) // Skip values that are already present in existing Entity
                newValues = newValues.Where(v => editableVersionOfTheEntity.Attributes.All(ev => ev.Value.Name != v.Key))
                    .ToDictionary(v => v.Key, v => v.Value);

            Context.Entities.UpdateEntity(editableVersionOfTheEntity.RepositoryId, newValues, updateLog: _importLog,
                preserveUndefinedValues: _keepAttributesMissingInImport, isPublished: importEntity.IsPublished, forceNoBranch: importEntity.ForceNoBranch);

            #endregion
        }
    }
}