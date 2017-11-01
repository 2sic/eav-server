using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbContentType
    {

        public void AddOrUpdate(string staticName, string scope, string name, string description, int? usesConfigurationOfOtherSet, bool alwaysShareConfig, bool changeStaticName = false, string newStaticName = "")
        {
            var ct = GetTypeByStaticName(staticName);

            if (ct == null)
            {
                ct = new ToSicEavAttributeSets()
                {
                    AppId = DbContext.AppId,
                    StaticName = Guid.NewGuid().ToString(),// staticName,
                    Scope = scope == "" ? null : scope,
                    UsesConfigurationOfAttributeSet = usesConfigurationOfOtherSet,
                    AlwaysShareConfiguration = alwaysShareConfig
                };
                DbContext.SqlDb.Add(ct);
            }

            ct.Name = name;
            ct.Description = description;
            ct.Scope = scope;
            if (changeStaticName) // note that this is a very "deep" change
                ct.StaticName = newStaticName;
            ct.ChangeLogCreated = DbContext.Versioning.GetChangeLogId();

            // save first, to ensure it has an Id
            DbContext.SqlDb.SaveChanges();
        }



        public void Delete(string staticName)
        {
            var setToDelete = GetTypeByStaticName(staticName);

            setToDelete.ChangeLogDeleted = DbContext.Versioning.GetChangeLogId();
            DbContext.SqlDb.SaveChanges();
        }

        


        internal int? GetOrCreateContentType(ContentType contentType)
        {
            var destinationSet = DbContext.AttribSet.GetDbAttribSet(contentType.StaticName);

            // add new AttributeSet, do basic configuration if possible, then save
            if (destinationSet == null)
                destinationSet = DbContext.AttribSet.PrepareDbAttribSet(contentType.Name, contentType.Description,
                    contentType.StaticName, contentType.Scope, false, null);

            // to use existing attribute Set, do some minimal conflict-checking
            else
            {
                DbContext.ImportLogToBeRefactored.Add(new LogItem(EventLogEntryType.Information, $"AttributeSet already exists{contentType.StaticName}|{contentType.Name}"));
                if (destinationSet.UsesConfigurationOfAttributeSet.HasValue)
                {
                    DbContext.ImportLogToBeRefactored.Add(new LogItem(EventLogEntryType.Error, "Not allowed to import/extend an AttributeSet which uses Configuration of another AttributeSet: " + contentType.StaticName));
                    return null;
                }
            }

            // If a "Ghost"-content type is specified, try to assign that
            if (!string.IsNullOrEmpty(contentType.OnSaveUseParentStaticName))
            {
                var ghostParentId = FindGhostParentIdOrLogWarnings(contentType.OnSaveUseParentStaticName);
                if (ghostParentId == 0) return null;
                destinationSet.UsesConfigurationOfAttributeSet = ghostParentId;
            }

            destinationSet.AlwaysShareConfiguration = contentType.AlwaysShareConfiguration;
            DbContext.SqlDb.SaveChanges();

            // if this set expects to share it's configuration, ensure that it does
            if (destinationSet.AlwaysShareConfiguration)
                DbContext.AttribSet.DistributeSharedContentTypes();

            // all ok :)
            return destinationSet.AttributeSetId;
        }



        internal void ExtendSaveContentTypes(List<IContentType> contentTypes, SaveOptions saveOptions)
            => DbContext.DoWhileQueueingRelationships(() => contentTypes.Cast<ContentType>().ToList().ForEach(ct => ExtendSaveContentTypes(ct, saveOptions)));

        /// <summary>
        /// Import an AttributeSet with all Attributes and AttributeMetaData
        /// </summary>
        private void ExtendSaveContentTypes(ContentType contentType, SaveOptions saveOptions)
        {
            // initialize destinationSet - create or test existing if ok
            var foundSet = GetOrCreateContentType(contentType);
            if (foundSet == null) // something went wrong, skip this import
                return;
            var contentTypeId = foundSet.Value;

            // append all Attributes
            foreach (var newAtt in contentType.Attributes.Cast<AttributeDefinition>())
            {
                var destAttribId = DbContext.AttributesDefinition.GetOrCreateAttributeDefinition(contentTypeId, newAtt);

                // save additional entities containing AttributeMetaData for this attribute
                if (newAtt.Metadata != null)
                    SaveAttributeMetadata(destAttribId, newAtt.Metadata, saveOptions);
            }

            // optionally re-order the attributes if specified in import
            if (contentType.OnSaveSortAttributes)
                SortAttributes(contentTypeId, contentType);
        }


        /// <summary>
        /// Save additional entities describing the attribute
        /// </summary>
        /// <param name="attributeId"></param>
        /// <param name="metadata"></param>
        /// <param name="saveOptions"></param>
        private void SaveAttributeMetadata(int attributeId, IEnumerable<IEntity> metadata, SaveOptions saveOptions)
        {
            var entities = new List<IEntity>();
            foreach (var entity in metadata)
            {
                var md = (Metadata)entity.Metadata;
                // Validate Entity
                md.TargetType = Constants.MetadataForAttribute;

                // Set KeyNumber
                if (attributeId == 0 || attributeId < 0) // < 0 is ef-core temp id
                    throw new Exception($"trying to add metadata to attribute {attributeId} but attribute isn't saved yet");

                md.KeyNumber = attributeId;

                entities.Add(entity);
            }
            DbContext.Save(entities, saveOptions); // don't use the standard save options, as this is attributes only
        }

    }
}
