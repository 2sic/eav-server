using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ToSic.Eav.ImportExport.Logging;
using ToSic.Eav.ImportExport.Models;
using ToSic.Eav.Persistence.EFC11.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    /// <summary>
    /// Import Schema and Entities to the EAV SqlStore
    /// </summary>
    public class DbImport
    {
        #region Private Fields
        private readonly DbDataController _context;
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
        public DbImport(int? zoneId, int? appId, bool dontUpdateExistingAttributeValues = true, bool keepAttributesMissingInImport = true, bool preventUpdateOnDraftEntities = true, bool largeImport = true)
        {
            _context = DbDataController.Instance(zoneId, appId);

            // 2017-04-04 2dm removed this, as it's now dependency injected into the _context
            //_context.UserName = userName;
            _dontUpdateExistingAttributeValues = dontUpdateExistingAttributeValues;
            _keepAttributesMissingInImport = keepAttributesMissingInImport;
            PreventUpdateOnDraftEntities = preventUpdateOnDraftEntities;
            _largeImport = largeImport;
        }

        /// <summary>
        /// Import AttributeSets and Entities
        /// </summary>
        internal /*IDbContextTransaction*/ void ImportIntoDb(IEnumerable<ImpAttrSet> newAttributeSets, IEnumerable<ImpEntity> newEntities)
        {
            _context.PurgeAppCacheOnSave = false;

            // Enhance the SQL timeout for imports
            // todo 2dm/2tk - discuss, this shouldn't be this high on a normal save, only on a real import
            // todo: on any error, cancel/rollback the transaction
            if (_largeImport)
                _context.SqlDb.Database.SetCommandTimeout(3600);//.CommandTimeout = 3600;

            // todo: CleanImport - change access to attribute set to use DB
            // Ensure cache is created
            // ReSharper disable once NotAccessedVariable
            //var y = DataSource.GetCache(Constants.DefaultZoneId, Constants.MetaDataAppId).LightList.Any();
            //var cache = DataSource.GetCache(_context.ZoneId, _context.AppId);
            //cache.PurgeCache(_context.ZoneId, _context.AppId);
            //// ReSharper disable once RedundantAssignment
            //y = cache.LightList.Any(); // re-read something

            #region initialize DB connection / transaction
            // Make sure the connection is open - because on multiple calls it's not clear if it was already opened or not
            var con = _context.SqlDb.Database.GetDbConnection(); // _context.SqlDb.Database.Connection
            if (con.State != ConnectionState.Open)
                con.Open();

            var transaction = _context.SqlDb.Database/*.Connection*/.BeginTransaction();

            #endregion

            try // run import, but rollback transaction if necessary
            {

                #region import AttributeSets if any were included

                if (newAttributeSets != null)
                {
                    var newSetsList = newAttributeSets.ToList();
                    // first: import the attribute sets in the system scope, as they may be needed by others...
                    // ...and would need a cache-refresh before 
                    var sysAttributeSets = newSetsList.Where(a => a.Scope == Constants.ScopeSystem).ToList();
                    if(sysAttributeSets.Any())
                        transaction = ImportSomeAttributeSets(sysAttributeSets, transaction);

                    // now the remaining attributeSets
                    var nonSysAttribSets = newSetsList.Where(a => !sysAttributeSets.Contains(a)).ToList();
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
                _context.SqlDb.Database.CloseConnection();// .Connection.Close();

                // todo: CleanImport - change access to attribute set to use DB
                // always Purge Cache
                //DataSource.GetCache(_context.ZoneId, _context.AppId).PurgeCache(_context.ZoneId, _context.AppId);

                // return transaction;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        private IDbContextTransaction ImportSomeAttributeSets(IEnumerable<ImpAttrSet> newAttributeSets, IDbContextTransaction transaction)
        {
            foreach (var attributeSet in newAttributeSets)
                ImportAttributeSet(attributeSet);

            _context.Relationships.ImportEntityRelationshipsQueue();

            _context.AttribSet.EnsureSharedAttributeSets();

            _context.SqlDb.SaveChanges();

            // Commit DB Transaction and refresh cache
            transaction.Commit();

            // todo: CleanImport - change access to attribute set to use DB
            //// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            //DataSource.GetCache(Constants.DefaultZoneId, Constants.MetaDataAppId).LightList.Any();
            //var cache = DataSource.GetCache(_context.ZoneId, _context.AppId);
            //cache.PurgeCache(_context.ZoneId, _context.AppId);
            //// ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            //cache.LightList.Any(); // re-read something

            // re-start transaction
            transaction = _context.SqlDb.Database/*.Connection*/.BeginTransaction();
            return transaction;
        }

        /// <summary>
        /// Import an AttributeSet with all Attributes and AttributeMetaData
        /// </summary>
        private void ImportAttributeSet(ImpAttrSet impAttrSet)
        {
            var destinationSet = _context.AttribSet.GetAttributeSet(impAttrSet.StaticName);
            // add new AttributeSet
            if (destinationSet == null)
                destinationSet = _context.AttribSet.AddContentTypeAndSave(impAttrSet.Name, impAttrSet.Description, impAttrSet.StaticName, impAttrSet.Scope, false);
            else	// use/update existing attribute Set
            {
                if (destinationSet.UsesConfigurationOfAttributeSet.HasValue)
                {
                    _importLog.Add(new ImportLogItem(EventLogEntryType.Error, "Not allowed to import/extend an AttributeSet which uses Configuration of another AttributeSet.") { ImpAttrSet = impAttrSet });
                    return;
                }

                _importLog.Add(new ImportLogItem(EventLogEntryType.Information, "AttributeSet already exists") { ImpAttrSet = impAttrSet });
            }

	        destinationSet.AlwaysShareConfiguration = impAttrSet.AlwaysShareConfiguration;

            // If a "Ghost"-content type is specified, try to assign that
            if (!string.IsNullOrEmpty(impAttrSet.UsesConfigurationOfAttributeSet))
            {
                // Look for a content type with the StaticName, which has no "UsesConfigurationOf..." set (is a master)
                var ghostAttributeSets = _context.SqlDb.ToSicEavAttributeSets.Where(a => a.StaticName == impAttrSet.UsesConfigurationOfAttributeSet && a.ChangeLogDeleted == null && a.UsesConfigurationOfAttributeSet == null).
                    OrderBy(a => a.AttributeSetId).ToList();

                if (ghostAttributeSets.Count == 0)
                {
                    _importLog.Add(new ImportLogItem(EventLogEntryType.Warning, "AttributeSet not imported because master set not found: " + impAttrSet.UsesConfigurationOfAttributeSet) { ImpAttrSet = impAttrSet });
                    return;
                }

                // If multiple masters are found, use first and add a warning message
                if (ghostAttributeSets.Count > 1)
                    _importLog.Add(new ImportLogItem(EventLogEntryType.Warning, "Multiple potential master AttributeSets found for StaticName: " + impAttrSet.UsesConfigurationOfAttributeSet) { ImpAttrSet = impAttrSet });
                destinationSet.UsesConfigurationOfAttributeSet = ghostAttributeSets.First().AttributeSetId;
            }
            

            if (destinationSet.AlwaysShareConfiguration)
	        {
		        _context.AttribSet.EnsureSharedAttributeSets();
	        }
	        _context.SqlDb.SaveChanges();

            // append all Attributes
            foreach (var importAttribute in impAttrSet.Attributes)
            {
                ToSicEavAttributes destinationAttribute;
                if(!_context.Attributes.Exists(destinationSet.AttributeSetId, importAttribute.StaticName))
                {
                        // try to add new Attribute
                        var isTitle = importAttribute == impAttrSet.TitleAttribute;
                    destinationAttribute = _context.Attributes.AppendAttribute(destinationSet, importAttribute.StaticName, importAttribute.Type, importAttribute.InputType, isTitle, false);
                }
				else
                {
					_importLog.Add(new ImportLogItem(EventLogEntryType.Warning, "Attribute already exists") { ImpAttribute = importAttribute });
                    destinationAttribute = destinationSet.ToSicEavAttributesInSets.Single(a => a.Attribute.StaticName == importAttribute.StaticName).Attribute;
                }

                // Insert AttributeMetaData
                if (/* isNewAttribute && */ importAttribute.AttributeMetaData != null)
                {
                    foreach (var entity in importAttribute.AttributeMetaData)
                    {
                        // Validate Entity
                        entity.KeyTypeId = Constants.MetadataForField;//.AssignmentObjectTypeIdFieldProperties;

                        // Set KeyNumber
                        if (destinationAttribute.AttributeId == 0)
                            _context.SqlDb.SaveChanges();
                        entity.KeyNumber = destinationAttribute.AttributeId;

                        // Get guid of previously existing assignment - if it exists
                        // todo: CleanImport - change access to attribute set to use DB
                        // todo: TestImport
                        var existingMetadata = _context.Entities
                            .GetAssignedEntities(Constants.MetadataForField,keyNumber:destinationAttribute.AttributeId)
                            .FirstOrDefault(e => e.AttributeSetId == destinationAttribute.AttributeId);

                        if (existingMetadata != null)
                            entity.EntityGuid = existingMetadata.EntityGuid;

                        //var mds = DataSource.GetMetaDataSource(_context.ZoneId, _context.AppId);
                        //var existingEntity = mds.GetAssignedEntities(Constants.MetadataForField, //.AssignmentObjectTypeIdFieldProperties,
                        //    destinationAttribute.AttributeId, entity.AttributeSetStaticName).FirstOrDefault();

                        //if (existingEntity != null)
                        //    entity.EntityGuid = existingEntity.EntityGuid;

                        PersistOneImportEntity(entity);
                    }
                }
            }

            // todo: optionally re-order the attributes
            if (impAttrSet.SortAttributes)
            {
                var attributeList = _context.SqlDb.ToSicEavAttributesInSets
                    .Where(a => a.AttributeSetId == destinationSet.AttributeSetId).ToList();
                attributeList = attributeList
                    .OrderBy(a => impAttrSet.Attributes
                        .IndexOf(impAttrSet.Attributes
                            .First(ia => ia.StaticName == a.Attribute.StaticName)))
                    .ToList();
                _context.Attributes.PersistAttributeSorting(attributeList);
            }
        }

        /// <summary>
        /// Import an Entity with all values
        /// </summary>
        private void PersistOneImportEntity(ImpEntity impEntity)
        {
            //var cache = DataSource.GetCache(null, _context.AppId);

            #region try to get AttributeSet or otherwise cancel & log error

            // var attributeSet = Context.AttribSet.GetAttributeSet(importEntity.AttributeSetStaticName);

            // todo: CleanImport - change access to attribute set to use DB
            var dbAttrSet = _context.AttribSet.GetAttributeSet(impEntity.AttributeSetStaticName);

            //var attributeSet = cache.GetContentType(impEntity.AttributeSetStaticName);
            if (/*attributeSet*/ dbAttrSet == null) // AttributeSet not Found
            {
                _importLog.Add(new ImportLogItem(EventLogEntryType.Error, "AttributeSet not found")
                {
                    ImpEntity = impEntity,
                    ImpAttrSet = new ImpAttrSet {StaticName = impEntity.AttributeSetStaticName}
                });
                return;
            }

            #endregion

            // Find existing Enties - meaning both draft and non-draft
            //List<IEntity> cacheExistingEntities = null;
            List<ToSicEavEntities> dbExistingEntities = null;
            if (impEntity.EntityGuid.HasValue)
            {
                dbExistingEntities = _context.Entities.GetEntitiesByGuid(impEntity.EntityGuid.Value).ToList();
                //cacheExistingEntities = cache.LightList.Where(e => e.EntityGuid == impEntity.EntityGuid.Value).ToList();
            }

            #region Simplest case - add (nothing existing to update)
            if (/*cacheExistingEntities*/dbExistingEntities == null || !/*cacheExistingEntities*/dbExistingEntities.Any())
            {
                _context.Entities.AddImportEntity(/*attributeSet.AttributeSetId*/ dbAttrSet.AttributeSetId, impEntity, _importLog, impEntity.IsPublished, null);
                return;
            }

            #endregion

            // todo: 2dm 2016-06-29 check this
            #region Another simple case - we have published entities, but are saving unpublished - so we create a new one

            if (!impEntity.IsPublished && /*cacheExistingEntities*/dbExistingEntities.Count(e => e.IsPublished == false) == 0 && !impEntity.ForceNoBranch)
            {
                var publishedId = dbExistingEntities.First().EntityId;// cacheExistingEntities.First().EntityId;
                _context.Entities.AddImportEntity(/*attributeSet.AttributeSetId*/ dbAttrSet.AttributeSetId, impEntity, _importLog, impEntity.IsPublished, publishedId);
                return;
            }

            #endregion 
             
            #region Update-Scenario - much more complex to decide what to change/update etc.

            #region Do Various Error checking like: Does it really exist, is it not draft, ensure we have the correct Content-Type

            // Get existing, published Entity
            var editableVersionOfTheEntity = /*cacheExistingEntities*/dbExistingEntities.OrderBy(e => e.IsPublished ? 1 : 0).First(); // get draft first, otherwise the published
            _importLog.Add(new ImportLogItem(EventLogEntryType.Information, "Entity already exists", impEntity));
        

            #region ensure we don't save a draft is this is not allowed (usually in the case of xml-import)

            // Prevent updating Draft-Entity - since the initial would be draft if it has one, this would throw
            if (PreventUpdateOnDraftEntities && !editableVersionOfTheEntity.IsPublished)
            {
                _importLog.Add(new ImportLogItem(EventLogEntryType.Error, "Importing a Draft-Entity is not allowed", impEntity));
                return;
            }

            #endregion

            #region Ensure entity has same AttributeSet (do this after checking for the draft etc.
            var editableEntityContentType = _context.AttribSet.GetAttributeSet(editableVersionOfTheEntity.AttributeSetId);
            if (editableEntityContentType.StaticName != impEntity.AttributeSetStaticName)
            //if (editableVersionOfTheEntity.Type.StaticName != impEntity.AttributeSetStaticName)
            {
                _importLog.Add(new ImportLogItem(EventLogEntryType.Error, "Existing entity (which should be updated) has different ContentType", impEntity));
                return;
            }
            #endregion



            #endregion

            // todo: TestImport - ensure that it correctly skips the existing values
            var newValues = impEntity.Values;
            if (_dontUpdateExistingAttributeValues) // Skip values that are already present in existing Entity
                newValues = newValues.Where(v => editableVersionOfTheEntity.ToSicEavValues/*.Attributes*/.All(ev => ev.Attribute.StaticName/*.Value.Name*/ != v.Key))
                    .ToDictionary(v => v.Key, v => v.Value);

            // todo: TestImport - ensure that the EntityId of this is what previously was the RepositoryID
            _context.Entities.UpdateEntity(editableVersionOfTheEntity.EntityId/*RepositoryId*/, newValues, updateLog: _importLog,
                preserveUndefinedValues: _keepAttributesMissingInImport, isPublished: impEntity.IsPublished, forceNoBranch: impEntity.ForceNoBranch);

            #endregion
        }
    }
}