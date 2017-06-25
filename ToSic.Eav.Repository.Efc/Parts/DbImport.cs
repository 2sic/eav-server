﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Persistence.Logging;
using Entity = ToSic.Eav.Data.Entity;

namespace ToSic.Eav.Repository.Efc.Parts
{
    /// <summary>
    /// Import Schema and Entities to the EAV SqlStore
    /// </summary>
    public class DbImport
    {
        #region Private Fields
        private readonly DbDataController _context;
        private AppDataPackage _entireApp;

        public SaveOptions SaveOptions = new SaveOptions();
        #endregion

        #region Properties

        /// <summary>
        /// Get the Import Log
        /// </summary>
        public List<ImportLogItem> ImportLog => _context.ImportLog;

        #endregion

        /// <summary>
        /// Initializes a new instance of the Import class.
        /// </summary>
        public DbImport(int? zoneId, int? appId, bool dontUpdateExistingAttributeValues = true, bool keepAttributesMissingInImport = true)
        {
            if(appId == null)
                throw new Exception("appid is null, can't continue with import");

            _context = DbDataController.Instance(zoneId, appId);

            SaveOptions.SkipExistingAttributes = dontUpdateExistingAttributeValues;

            SaveOptions.PreserveUntouchedAttributes = keepAttributesMissingInImport;

            SaveOptions.Languages = _context.Dimensions.GetLanguageListForImport(SaveOptions.PrimaryLanguage).ToList();

        }

        /// <summary>
        /// Import AttributeSets and Entities
        /// </summary>
        public void ImportIntoDb(IEnumerable<ContentType> newAttributeSets, IEnumerable<Entity> newEntities)
        {
            _context.PurgeAppCacheOnSave = false;

            #region initialize DB connection / transaction
            // Make sure the connection is open - because on multiple calls it's not clear if it was already opened or not
            var con = _context.SqlDb.Database.GetDbConnection();
            if (con.State != ConnectionState.Open)
                con.Open();

            var transaction = _context.SqlDb.Database.BeginTransaction();

            // get initial data state for further processing, content-typed definitions etc.
            // important: must always create a new loader, because it will cache content-types which hurts the import
            _entireApp = new Efc11Loader(_context.SqlDb).AppPackage(_context.AppId, new [] {0}); // don't load any entities, just content-types

            #endregion
            // run import, but rollback transaction if necessary
            try
            {
                #region import AttributeSets if any were included
                if (newAttributeSets != null)
                    _context.Versioning.QueueDuringAction(() => {
                        var newSetsList = newAttributeSets.ToList();
                        // first: import the attribute sets in the system scope, as they may be needed by others...
                        // ...and would need a cache-refresh before 
                        var sysAttributeSets = newSetsList.Where(a => a.Scope == Constants.ScopeSystem).ToList();
                        if (sysAttributeSets.Any())
                            ImportSomeAttributeSets(sysAttributeSets);

                        _entireApp = new Efc11Loader(_context.SqlDb).AppPackage(_context.AppId, new[] { 0 }); // don't load any entities, just content-types

                        // now the remaining attributeSets
                        var nonSysAttribSets = newSetsList.Where(a => !sysAttributeSets.Contains(a)).ToList();
                        if (nonSysAttribSets.Any())
                            ImportSomeAttributeSets(nonSysAttribSets);
                    });

                #endregion

                #region import Entities

                if (newEntities != null)
                {
                    _entireApp = new Efc11Loader(_context.SqlDb).AppPackage(_context.AppId); // load all entities
                    newEntities = newEntities.Select(PrepareEntityForImport).Where(e => e != null).ToList();
                    _context.Entities.SaveEntity(newEntities.Cast<IEntity>().ToList(), SaveOptions);
                }

                #endregion

                // Commit DB Transaction
                transaction.Commit();
                _context.SqlDb.Database.CloseConnection();

            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        private void ImportSomeAttributeSets(IEnumerable<ContentType> newAttributeSets)
        {
            foreach (var attributeSet in newAttributeSets)
                ImportAttributeSet(attributeSet);

            _context.Relationships.ImportRelationshipQueueAndSave();

            // in case anything imported was to be shared, ensure that
            _context.AttribSet.EnsureSharedAttributeSetsOnEverythingAndSave();
        }

        /// <summary>
        /// Import an AttributeSet with all Attributes and AttributeMetaData
        /// </summary>
        private void ImportAttributeSet(ContentType contentType)
        {
            // initialize destinationSet - create or test existing if ok
            var destinationSet = GetAndCheckIfValidOrCreateDestinationSet(contentType);
            if (destinationSet == null) // something went wrong, skip this import
                return;

            // if this set expects to share it's configuration, ensure that it does
            if (destinationSet.AlwaysShareConfiguration)
                _context.AttribSet.EnsureSharedAttributeSetsOnEverythingAndSave();

            // append all Attributes
            foreach (AttributeDefinition newAtt in contentType.Attributes)
            {
                int destAttribId;
                if(!_context.AttributesDefinition.AttributeExistsInSet(destinationSet.AttributeSetId, newAtt.Name))
                {
                    // try to add new Attribute
                    var isTitle = newAtt.IsTitle;// == impContentType.TitleAttribute;
                    destAttribId = _context.AttributesDefinition
                        .AppendToEndAndSave(destinationSet, 0, newAtt.Name, newAtt.Type, isTitle);
                }
				else
                {
					ImportLog.Add(new ImportLogItem(EventLogEntryType.Warning, "Attribute already exists") { ImpAttribute = newAtt.Name });
                    destAttribId = destinationSet.ToSicEavAttributesInSets
                        .Single(a => a.Attribute.StaticName == newAtt.Name).Attribute.AttributeId;
                }

                // save additional entities containing AttributeMetaData for this attribute
                if (newAtt.InternalAttributeMetaData != null)
                    SaveImportedAttributeMetadata(newAtt.InternalAttributeMetaData, destAttribId);
            }

            // optionally re-order the attributes if specified in import
            if (contentType.OnSaveSortAttributes)
                SortAttributesByImportOrder(contentType, destinationSet);
        }

        private ToSicEavAttributeSets GetAndCheckIfValidOrCreateDestinationSet(ContentType contentType)
        {
            var destinationSet = _context.AttribSet.GetDbAttribSet(contentType.StaticName);

            // add new AttributeSet, do basic configuration if possible, then save
            if (destinationSet == null)
                destinationSet = _context.AttribSet.PrepareDbAttribSet(contentType.Name, contentType.Description,
                    contentType.StaticName, contentType.Scope, false, null);

            // to use existing attribute Set, do some minimal conflict-checking
            else
            {
                ImportLog.Add(new ImportLogItem(EventLogEntryType.Information, "AttributeSet already exists")
                {
                    ContentType = contentType
                });
                if (destinationSet.UsesConfigurationOfAttributeSet.HasValue)
                {
                    ImportLog.Add(new ImportLogItem(EventLogEntryType.Error,
                        "Not allowed to import/extend an AttributeSet which uses Configuration of another AttributeSet.")
                    {
                        ContentType = contentType
                    });
                    return null;
                }
            }

            // If a "Ghost"-content type is specified, try to assign that
            if (!string.IsNullOrEmpty(contentType.OnSaveUseParentStaticName))
            {
                var ghostParentId = FindCorrectGhostParentId(contentType.OnSaveUseParentStaticName);
                if (ghostParentId == 0) return null;
                destinationSet.UsesConfigurationOfAttributeSet = ghostParentId;
            }

            destinationSet.AlwaysShareConfiguration = contentType.AlwaysShareConfiguration;
            _context.SqlDb.SaveChanges();

            // all ok :)
            return destinationSet;
        }

        /// <summary>
        /// Look up the ghost-parent-id
        /// </summary>
        /// <returns>The parent id as needed, or 0 if not found - which usually indicates an import problem</returns>
        private int FindCorrectGhostParentId(string contentTypeParentName)
        {
            // Look for the potential source of this ghost
            var ghostAttributeSets = _context.ContentType.FindPotentialGhostSources(contentTypeParentName);

            if(ghostAttributeSets.Count == 1)
                return ghostAttributeSets.First().AttributeSetId;

            // If multiple masters are found, use first and add a warning message
            if (ghostAttributeSets.Count > 1)
                ImportLog.Add(new ImportLogItem(EventLogEntryType.Warning, $"Multiple potential master AttributeSets found for StaticName: {contentTypeParentName}") );
            
            // nothing found - report error
            ImportLog.Add(new ImportLogItem(EventLogEntryType.Warning, $"AttributeSet not imported because master set not found: {contentTypeParentName}"));
            return 0;
        }


        /// <summary>
        /// Save additional entities describing the attribute
        /// </summary>
        /// <param name="attributeMetaData"></param>
        /// <param name="destinationAttributeId"></param>
        private void SaveImportedAttributeMetadata(List<Entity> attributeMetaData, int destinationAttributeId)
        {
            var entities = new List<IEntity>();
            foreach (var entity in attributeMetaData)
            {
                var md = (Metadata) entity.Metadata;
                // Validate Entity
                md.TargetType = Constants.MetadataForField;

                // Set KeyNumber
                if (destinationAttributeId == 0 || destinationAttributeId < 0) // < 0 is ef-core temp id
                    _context.SqlDb.SaveChanges();
                md.KeyNumber = destinationAttributeId;

                // Get guid of previously existing assignment - if it exists
                var existingMetadata = _context.Entities
                    .GetAssignedEntities(Constants.MetadataForField, keyNumber: destinationAttributeId)
                    .FirstOrDefault(e => e.AttributeSetId == destinationAttributeId);

                if (existingMetadata != null)
                    entity.SetGuid(existingMetadata.EntityGuid);

                entities.Add(PrepareEntityForImport(entity));
            }
            _context.Entities.SaveEntity(entities, new SaveOptions()); // don't use the standard save options, as this is attributes only
        }

        /// <summary>
        /// Sometimes the import asks for sorting the fields again according to input
        /// this method will then take care of re-sorting them correctly
        /// Fields which were not in the import will simply land at the end
        /// </summary>
        /// <param name="contentType"></param>
        /// <param name="destinationSet"></param>
        private void SortAttributesByImportOrder(ContentType contentType, ToSicEavAttributeSets destinationSet)
        {
            var attributeList = _context.SqlDb.ToSicEavAttributesInSets
                .Where(a => a.AttributeSetId == destinationSet.AttributeSetId)
                .ToList();

            attributeList = attributeList
                .OrderBy(a => contentType.Attributes
                    .IndexOf(contentType.Attributes
                        .First(ia => ia.Name == a.Attribute.StaticName)))
                .ToList();
            _context.AttributesDefinition.PersistAttributeOrder(attributeList);
        }

        /// <summary>
        /// Import an Entity with all values
        /// </summary>
        private Entity PrepareEntityForImport(Entity entity)
        {
            // no types of Draft-imports allowed
            if (!entity.IsPublished) // AttributeSet not Found
            {
                ImportLog.Add(new ImportLogItem(EventLogEntryType.Error, "Found draft, but can never import draft-items: " + entity.EntityGuid));
                return null;
            }

           #region try to get AttributeSet or otherwise cancel & log error

            var dbAttrSet = _entireApp.ContentTypes.Values
                .FirstOrDefault(ct => String.Equals(ct.StaticName, entity.Type.StaticName, StringComparison.InvariantCultureIgnoreCase));

            if (dbAttrSet == null) // AttributeSet not Found
            {
                ImportLog.Add(new ImportLogItem(EventLogEntryType.Error, "AttributeSet not found: " + entity.Type.StaticName));
                return null;
            }

            #endregion

            // Find existing Enties - meaning both draft and non-draft
            List<IEntity> existingEntities = null;
            if (entity.EntityGuid != Guid.Empty)
                existingEntities = _entireApp.List.Where(e => e.EntityGuid == entity.EntityGuid).ToList(); // _context.Entities.GetEntitiesByGuid(entity.EntityGuid).ToList();

            #region Simplest case - nothing existing to update: return entity

            if (existingEntities == null || !existingEntities.Any())
                return entity;// _context.Entities.SaveEntity(entity, SaveOptions);

            #endregion

            ImportLog.Add(new ImportLogItem(EventLogEntryType.Information, $"FYI: Entity {entity.EntityId} already exists for guid {entity.EntityGuid}"));

            #region ensure we don't save on a draft is this is not allowed (usually in the case of xml-import)

            // Prevent updating Draft-Entity - since the initial would be draft if it has one, this would throw
            if (existingEntities.Any(e => !e.IsPublished))
            {
                ImportLog.Add(new ImportLogItem(EventLogEntryType.Error, $"Importing on Draft-Entity is not allowed - this entity drafts: {entity.EntityGuid}"));
                return null;
            }

            #endregion

            // now update (main) entity id from existing - since it already exists
            var original = existingEntities.First();
            entity.ChangeIdForSaving(original.EntityId);
            return EntitySaver.CreateMergedForSaving(original, entity, SaveOptions) as Entity;

        }
    }
}