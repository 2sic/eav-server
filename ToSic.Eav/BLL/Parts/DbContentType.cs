using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Api;
using ToSic.Eav.Data;
using ToSic.Eav.Import;

namespace ToSic.Eav.BLL.Parts
{
    public class DbContentType: BllCommandBase
    {
        public DbContentType(EavDataController cntx) : base(cntx) {}


        private AttributeSet GetAttributeSetByStaticName(string staticName)
        {
            return Context.SqlDb.AttributeSets.FirstOrDefault(a =>
                a.AppID == Context.AppId && a.StaticName == staticName && a.ChangeLogDeleted == null
                );
        }

        public void AddOrUpdate(string staticName, string scope, string name, string description, int? usesConfigurationOfOtherSet, bool alwaysShareConfig, bool changeStaticName = false, string newStaticName = "")
        {
            var ct = GetAttributeSetByStaticName(staticName);

            if (ct == null)
            {
                ct = new AttributeSet()
                {
                    AppID = Context.AppId,
                    StaticName = Guid.NewGuid().ToString(),// staticName,
                    Scope = scope == "" ? null : scope,
                    UsesConfigurationOfAttributeSet = usesConfigurationOfOtherSet,
                    AlwaysShareConfiguration = alwaysShareConfig
                };
                Context.SqlDb.AddToAttributeSets(ct);
            }

            ct.Name = name;
            ct.Description = description;
            ct.Scope = scope;
            if (changeStaticName) // note that this is a very "deep" change
                ct.StaticName = newStaticName;
            ct.ChangeLogIDCreated = Context.Versioning.GetChangeLogId(Context.UserName);

            // save first, to ensure it has an Id
            Context.SqlDb.SaveChanges();

            //var fullApi = new BetaFullApi(Context.ZoneId, Context.AppId, Context);
            //fullApi.Metadata_AddOrUpdate(Constants.AssignmentObjectTypeIdFieldProperties, ct.AttributeSetID, "@All", newValues);

        }

        public void CreateGhost(string staticName)
        {
            var ct = GetAttributeSetByStaticName(staticName);
            if (ct != null)
                throw new Exception("current App already has a content-type with this static name - cannot continue");

            // find the original
            var attSets = Context.SqlDb.AttributeSets
                .Where(ats => ats.StaticName == staticName 
                    && !ats.UsesConfigurationOfAttributeSet.HasValue    // never duplicate a clone/ghost
                    && ats.ChangeLogDeleted == null                     // never duplicate a deleted
                    && ats.AlwaysShareConfiguration == false)           // never duplicate an always-share
                .OrderBy(ats => ats.AttributeSetID)
                .ToList();

            if(attSets.Count == 0)
                throw new ArgumentException("can't find an original, non-ghost content-type with the static name '" + staticName + "'");

            if (attSets.Count > 1)
                throw new Exception("found " + attSets.Count + " (expected 1) original, non-ghost content-type with the static name '" + staticName + "' - so won't create ghost as it's not clear off which you would want to ghost.");

            var attSet = attSets.FirstOrDefault();
            var newSet = new AttributeSet()
            {
                AppID = Context.AppId, // needs the new, current appid
                StaticName = attSet.StaticName,
                Name = attSet.Name,
                Scope = attSet.Scope,
                Description = attSet.Description,
                UsesConfigurationOfAttributeSet = attSet.AttributeSetID,
                AlwaysShareConfiguration = false, // this is copy, never re-share
                ChangeLogIDCreated = Context.Versioning.GetChangeLogId(Context.UserName)
            };
            Context.SqlDb.AddToAttributeSets(newSet);
            
            // save first, to ensure it has an Id
            Context.SqlDb.SaveChanges();

            //var fullApi = new BetaFullApi(Context.ZoneId, Context.AppId, Context);
            //fullApi.Metadata_AddOrUpdate(Constants.AssignmentObjectTypeIdFieldProperties, ct.AttributeSetID, "@All", newValues);

        }


        public void Delete(string staticName)
        {
            var setToDelete = GetAttributeSetByStaticName(staticName);

            setToDelete.ChangeLogIDDeleted = Context.Versioning.GetChangeLogId(Context.UserName);
            Context.SqlDb.SaveChanges();
        }


        /// <summary>
        /// Returns the configuration for a content type
        /// </summary>
        public IEnumerable<Tuple<IAttributeBase, Dictionary<string, IEntity>>> GetContentTypeConfiguration(string contentTypeStaticName)
        {
            var cache = DataSource.GetCache(null, Context.AppId);
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
                    .GetAssignedEntities(Constants.AssignmentObjectTypeIdFieldProperties, a.Value.AttributeId)
                    .ToDictionary(e => e.Type.StaticName.TrimStart('@'), e => e)
            });

            return config.Select(a => new Tuple<IAttributeBase, Dictionary<string, IEntity>>(a.Attribute, a.Metadata));
        }

        //public IEnumerable<dynamic> GetAttributeMetadataTypes()
        //{
        //    var metaDataSource = DataSource.GetMetaDataSource(Zonei, metaDataAppId);
        //}

        public void Reorder(int contentTypeId, int attributeId, string direction)
        {
            if (direction == "up")
            {
                Context.Attributes.ChangeAttributeOrder(attributeId, contentTypeId, AttributeMoveDirection.Up);
                return;
            }
            else if (direction == "down")
            {
                Context.Attributes.ChangeAttributeOrder(attributeId, contentTypeId, AttributeMoveDirection.Down);
                return;
            }
        }
    }
}
