using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbContentType: BllCommandBase
    {
        public DbContentType(DbDataController cntx) : base(cntx) {}


        private ToSicEavAttributeSets GetAttributeSetByStaticName(string staticName)
        {
            return DbContext.SqlDb.ToSicEavAttributeSets.FirstOrDefault(a =>
                a.AppId == DbContext.AppId && a.StaticName == staticName && a.ChangeLogDeleted == null
                );
        }

        public void AddOrUpdate(string staticName, string scope, string name, string description, int? usesConfigurationOfOtherSet, bool alwaysShareConfig, bool changeStaticName = false, string newStaticName = "")
        {
            var ct = GetAttributeSetByStaticName(staticName);

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

        public void CreateGhost(string staticName)
        {
            var ct = GetAttributeSetByStaticName(staticName);
            if (ct != null)
                throw new Exception("current App already has a content-type with this static name - cannot continue");

            // find the original
            var attSets = DbContext.SqlDb.ToSicEavAttributeSets
                .Where(ats => ats.StaticName == staticName 
                    && !ats.UsesConfigurationOfAttributeSet.HasValue    // never duplicate a clone/ghost
                    && ats.ChangeLogDeleted == null                     // never duplicate a deleted
                    && ats.AlwaysShareConfiguration == false)           // never duplicate an always-share
                .OrderBy(ats => ats.AttributeSetId)
                .ToList();

            if(attSets.Count() == 0)
                throw new ArgumentException("can't find an original, non-ghost content-type with the static name '" + staticName + "'");

            if (attSets.Count() > 1)
                throw new Exception("found " + attSets.Count() + " (expected 1) original, non-ghost content-type with the static name '" + staticName + "' - so won't create ghost as it's not clear off which you would want to ghost.");

            var attSet = attSets.FirstOrDefault();
            var newSet = new ToSicEavAttributeSets()
            {
                AppId = DbContext.AppId, // needs the new, current appid
                StaticName = attSet.StaticName,
                Name = attSet.Name,
                Scope = attSet.Scope,
                Description = attSet.Description,
                UsesConfigurationOfAttributeSet = attSet.AttributeSetId,
                AlwaysShareConfiguration = false, // this is copy, never re-share
                ChangeLogCreated = DbContext.Versioning.GetChangeLogId()
            };
            DbContext.SqlDb.Add(newSet);
            
            // save first, to ensure it has an Id
            DbContext.SqlDb.SaveChanges();
        }


        public void Delete(string staticName)
        {
            var setToDelete = GetAttributeSetByStaticName(staticName);

            setToDelete.ChangeLogDeleted = DbContext.Versioning.GetChangeLogId();
            DbContext.SqlDb.SaveChanges();
        }


        /// <summary>
        /// Returns the configuration for a content type
        /// </summary>
        public IEnumerable<Tuple<IAttributeBase, Dictionary<string, IEntity>>> GetContentTypeConfiguration(string contentTypeStaticName)
        {
            var cache = DataSource.GetCache(null, DbContext.AppId);
            var result = cache.GetContentType(contentTypeStaticName);

            if (result == null)
                throw new Exception("Content type " + contentTypeStaticName + " not found.");

            // Resolve ZoneId & AppId of the MetaData. If this AttributeSet uses configuration of another AttributeSet, use MetaData-ZoneId & -AppId
            var metaDataAppId = result.ConfigurationAppId;
            var metaDataZoneId = result.ConfigurationZoneId;

            var metaDataSource = DataSource.GetMetaDataSource(metaDataZoneId, metaDataAppId);

            var config = result.AttributeDefinitions.Select(a => new
            {
                Attribute = a.Value,
                Metadata = metaDataSource
                    .GetAssignedEntities(Constants.MetadataForField, a.Value.AttributeId)
                    .ToDictionary(e => e.Type.StaticName.TrimStart('@'), e => e)
            });

            return config.Select(a => new Tuple<IAttributeBase, Dictionary<string, IEntity>>(a.Attribute, a.Metadata));
        }
        
        public void Reorder(int contentTypeId, List<int> newSortOrder)
        {
            DbContext.Attributes.UpdateAttributeOrder(contentTypeId, newSortOrder);
        }
    }
}
