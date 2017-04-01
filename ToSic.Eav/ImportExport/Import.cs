using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.BLL;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Import
{
    /// <summary>
    /// Import Schema and Entities to the EAV SqlStore
    /// </summary>
    public class Import
    {
        #region Private Fields
        private readonly EavDataController _context;
        private readonly bool _dontUpdateExistingAttributeValues;
        private readonly bool _keepAttributesMissingInImport;
        private readonly List<ImportLogItem> _importLog = new List<ImportLogItem>();
        private readonly bool _largeImport;
        #endregion

        #region Properties
        /// <summary>
        /// Get the Import Log
        /// </summary>
        public List<ImportLogItem> ImportLog => _importLog;

        bool PreventUpdateOnDraftEntities { get; }
        #endregion

        /// <summary>
        /// Initializes a new instance of the Import class.
        /// </summary>
        public Import(int? zoneId, int? appId, string userName, bool dontUpdateExistingAttributeValues = true, bool keepAttributesMissingInImport = true, bool preventUpdateOnDraftEntities = true, bool largeImport = true)
        {
            _context = EavDataController.Instance(zoneId, appId);

            _context.UserName = userName;
            _dontUpdateExistingAttributeValues = dontUpdateExistingAttributeValues;
            _keepAttributesMissingInImport = keepAttributesMissingInImport;
            PreventUpdateOnDraftEntities = preventUpdateOnDraftEntities;
            _largeImport = largeImport;
        }

        /// <summary>
        /// Import AttributeSets and Entities
        /// </summary>
        public DbTransaction RunImport(IEnumerable<ImportAttributeSet> newAttributeSets, IEnumerable<ImportEntity> newEntities)
        {
            _context.PurgeAppCacheOnSave = false;

            // Enhance the SQL timeout for imports
            // todo 2dm/2tk - discuss, this shouldn't be this high on a normal save, only on a real import
            // todo: on any error, cancel/rollback the transaction
            if (_largeImport)
                _context.SqlDb.CommandTimeout = 3600;

            // Ensure cache is created
            // ReSharper disable once NotAccessedVariable
            var y = DataSource.GetCache(Constants.DefaultZoneId, Constants.MetaDataAppId).LightList.Any();
            var cache = DataSource.GetCache(_context.ZoneId, _context.AppId);
            cache.PurgeCache(_context.ZoneId, _context.AppId);
            // ReSharper disable once RedundantAssignment
            y = cache.LightList.Any(); // re-read something

            #region initialize DB connection / transaction
            // Make sure the connection is open - because on multiple calls it's not clear if it was already opened or not
            if (_context.SqlDb.Connection.State != ConnectionState.Open)
                _context.SqlDb.Connection.Open();

            var transaction = _context.SqlDb.Connection.BeginTransaction();

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

                    _context.Relationships.ImportEntityRelationshipsQueue();

                    _context.SqlDb.SaveChanges();
                }
                #endregion

                // Commit DB Transaction
                transaction.Commit();
                _context.SqlDb.Connection.Close();

                // always Purge Cache
                DataSource.GetCache(_context.ZoneId, _context.AppId).PurgeCache(_context.ZoneId, _context.AppId);

                return transaction;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        private DbTransaction ImportSomeAttributeSets(IEnumerable<ImportAttributeSet> newAttributeSets, DbTransaction transaction)
        {
            foreach (var attributeSet in newAttributeSets)
                ImportAttributeSet(attributeSet);

            _context.Relationships.ImportEntityRelationshipsQueue();

            _context.AttribSet.EnsureSharedAttributeSets();

            _context.SqlDb.SaveChanges();

            // Commit DB Transaction and refresh cache
            transaction.Commit();
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            DataSource.GetCache(Constants.DefaultZoneId, Constants.MetaDataAppId).LightList.Any();
            var cache = DataSource.GetCache(_context.ZoneId, _context.AppId);
            cache.PurgeCache(_context.ZoneId, _context.AppId);
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            cache.LightList.Any(); // re-read something

            // re-start transaction
            transaction = _context.SqlDb.Connection.BeginTransaction();
            return transaction;
        }

        /// <summary>
        /// Import an AttributeSet with all Attributes and AttributeMetaData
        /// </summary>
        private void ImportAttributeSet(ImportAttributeSet importAttributeSet)
        {
            var destinationSet = _context.AttribSet.GetAttributeSet(importAttributeSet.StaticName);
            // add new AttributeSet
            if (destinationSet == null)
                destinationSet = _context.AttribSet.AddAttributeSet(importAttributeSet.Name, importAttributeSet.Description, importAttributeSet.StaticName, importAttributeSet.Scope, false);
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
                var ghostAttributeSets = _context.SqlDb.AttributeSets.Where(a => a.StaticName == importAttributeSet.UsesConfigurationOfAttributeSet && a.ChangeLogDeleted == null && a.UsesConfigurationOfAttributeSet == null).
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
		        _context.AttribSet.EnsureSharedAttributeSets();
	        }
	        _context.SqlDb.SaveChanges();

            // append all Attributes
            foreach (var importAttribute in importAttributeSet.Attributes)
            {
                Attribute destinationAttribute;
                if(!_context.Attributes.Exists(destinationSet.AttributeSetID, importAttribute.StaticName))
                {
                        // try to add new Attribute
                        var isTitle = importAttribute == importAttributeSet.TitleAttribute;
                    destinationAttribute = _context.Attributes.AppendAttribute(destinationSet, importAttribute.StaticName, importAttribute.Type, importAttribute.InputType, isTitle, false);
                }
				else
                {
					_importLog.Add(new ImportLogItem(EventLogEntryType.Warning, "Attribute already exists") { ImportAttribute = importAttribute });
                    destinationAttribute = destinationSet.AttributesInSets.Single(a => a.Attribute.StaticName == importAttribute.StaticName).Attribute;
                }

                var mds = DataSource.GetMetaDataSource(_context.ZoneId, _context.AppId);
                // Insert AttributeMetaData
                if (/* isNewAttribute && */ importAttribute.AttributeMetaData != null)
                {
                    foreach (var entity in importAttribute.AttributeMetaData)
                    {
                        // Validate Entity
                        entity.AssignmentObjectTypeId = Constants.MetadataForField;//.AssignmentObjectTypeIdFieldProperties;

                        // Set KeyNumber
                        if (destinationAttribute.AttributeID == 0)
                            _context.SqlDb.SaveChanges();
                        entity.KeyNumber = destinationAttribute.AttributeID;

                        var existingEntity = mds.GetAssignedEntities(Constants.MetadataForField, //.AssignmentObjectTypeIdFieldProperties,
                            destinationAttribute.AttributeID, entity.AttributeSetStaticName).FirstOrDefault();

                        if (existingEntity != null)
                            entity.EntityGuid = existingEntity.EntityGuid;

                        PersistOneImportEntity(entity);
                    }
                }
            }

            // todo: optionally re-order the attributes
            if (importAttributeSet.SortAttributes)
            {
                var attributeList = _context.SqlDb.AttributesInSets
                    .Where(a => a.AttributeSetID == destinationSet.AttributeSetID).ToList();
                attributeList = attributeList
                    .OrderBy(a => importAttributeSet.Attributes
                        .IndexOf(importAttributeSet.Attributes
                            .First(ia => ia.StaticName == a.Attribute.StaticName)))
                    .ToList();
                _context.Attributes.PersistAttributeSorting(attributeList);
            }
        }

        /// <summary>
        /// Import an Entity with all values
        /// </summary>
        private void PersistOneImportEntity(ImportEntity importEntity)
        {
            var cache = DataSource.GetCache(null, _context.AppId);

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
                _context.Entities.AddEntity(attributeSet.AttributeSetId, importEntity, _importLog, importEntity.IsPublished, null);
                return;
            }

            #endregion

            // todo: 2dm 2016-06-29 check this
            #region Another simple case - we have published entities, but are saving unpublished - so we create a new one

            if (!importEntity.IsPublished && existingEntities.Count(e => e.IsPublished == false) == 0 && !importEntity.ForceNoBranch)
            {
                var publishedId = existingEntities.First().EntityId;
                _context.Entities.AddEntity(attributeSet.AttributeSetId, importEntity, _importLog, importEntity.IsPublished, publishedId);
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

            _context.Entities.UpdateEntity(editableVersionOfTheEntity.RepositoryId, newValues, updateLog: _importLog,
                preserveUndefinedValues: _keepAttributesMissingInImport, isPublished: importEntity.IsPublished, forceNoBranch: importEntity.ForceNoBranch);

            #endregion
        }
    }
}