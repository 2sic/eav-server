using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbPipeline: BllCommandBase
    {
        public DbPipeline(DbDataController c) : base(c, "Db.Pipe") { }

        /// <summary>
        /// Copy an existing DataPipeline by copying all Entities and uptdate their GUIDs
        /// </summary>
        public int CopyDataPipeline(int appId, int pipelineEntityId, string userName)
        {
            int returnId = 0;

            DbContext.DoInTransaction(() =>
            {
                // Clone Pipeline Entity with a new new Guid
                string importantIncludes = "AttributeSet,ToSicEavValues,ToSicEavValues.ToSicEavValuesDimensions";
                var sourcePipelineEntity =
                    DbContext.Entities.GetDbEntity(pipelineEntityId, importantIncludes + ",ToSicEavValues.Attribute");
                if (sourcePipelineEntity.AttributeSet.StaticName != Constants.DataPipelineStaticName)
                    throw new ArgumentException("Entity is not an DataPipeline Entity", nameof(pipelineEntityId));
                var pipelineEntityClone = CloneEntityForPipelineSimpleValuesOnly(sourcePipelineEntity);
                DbContext.SqlDb.SaveChanges(); // do this to ensure we have new IDs

                // Copy Pipeline Parts with configuration Entity, assign KeyGuid of the new Pipeline Entity
                var pipelineParts = DbContext.Entities.GetEntityMetadataByGuid(appId, sourcePipelineEntity.EntityGuid,
                    includes: importantIncludes);

                var pipelinePartClones = new Dictionary<string, Guid>(); // track Guids of originals and their clone
                foreach (var pipelinePart in pipelineParts)
                {
                    var pipelinePartClone = CloneEntityForPipelineSimpleValuesOnly(pipelinePart);
                    pipelinePartClone.KeyGuid = pipelineEntityClone.EntityGuid;
                    pipelinePartClones.Add(pipelinePart.EntityGuid.ToString(), pipelinePartClone.EntityGuid);

                    // Copy Configuration Entity, assign KeyGuid of the Clone
                    var configurationEntity = DbContext.Entities
                        .GetEntityMetadataByGuid(appId, pipelinePart.EntityGuid, includes: importantIncludes)
                        .SingleOrDefault();
                    if (configurationEntity != null)
                    {
                        var configurationClone = CloneEntityForPipelineSimpleValuesOnly(configurationEntity);
                        configurationClone.KeyGuid = pipelinePartClone.EntityGuid;

                        // todo: json-metadata-for...
                        // must switch entity-id
                        // must also switch guid
                    }
                }
                DbContext.SqlDb.SaveChanges();

                #region Update Stream-Wirings

                // need the wiring attribute id for later, as I can't detect it on the unsaved clone
                var wiringAttributeId = sourcePipelineEntity.ToSicEavValues
                    .Single(v => v.Attribute.StaticName == Constants.DataPipelineStreamWiringStaticName)
                    .AttributeId;

                var streamWiring = pipelineEntityClone.ToSicEavValues.Single(v => v.AttributeId == wiringAttributeId);
                var wiringsClone = new List<WireInfo>();
                var wiringsSource = DataPipelineWiring.Deserialize(streamWiring.Value);
                if (wiringsSource != null)
                    foreach (var wireInfo in wiringsSource)
                    {
                        var wireInfoClone = wireInfo; // creates a clone of the Struct
                        if (pipelinePartClones.ContainsKey(wireInfo.From))
                            wireInfoClone.From = pipelinePartClones[wireInfo.From].ToString();
                        if (pipelinePartClones.ContainsKey(wireInfo.To))
                            wireInfoClone.To = pipelinePartClones[wireInfo.To].ToString();

                        wiringsClone.Add(wireInfoClone);
                    }

                streamWiring.Value = DataPipelineWiring.Serialize(wiringsClone);

                #endregion

                DbContext.SqlDb.SaveChanges();
                returnId = pipelineEntityClone.EntityId;
            });
            return returnId;
        }

        #region Clone
        /// <summary>
        /// Clone an Entity with all Values
        /// This is only used for the pipeline
        /// and DOES NOT clone relationships
        /// note that we should refactor this soon, as soon as json-serialization etc. works
        /// with that, we should then clone using serialize/deserialize, and not a DB-level clone
        /// </summary>
        private ToSicEavEntities CloneEntityForPipelineSimpleValuesOnly(ToSicEavEntities sourceEntity)
        {
            var versioningId = DbContext.Versioning.GetChangeLogId();
            var clone = new ToSicEavEntities
            {
                EntityGuid = Guid.NewGuid(),
                AttributeSet = sourceEntity.AttributeSet,
                AssignmentObjectTypeId = sourceEntity.AssignmentObjectTypeId,
                KeyGuid = sourceEntity.KeyGuid,
                KeyNumber = sourceEntity.KeyNumber,
                KeyString = sourceEntity.KeyString,
                ChangeLogCreated = versioningId,
                ChangeLogModified = versioningId,
                Owner = sourceEntity.Owner,
                IsPublished = sourceEntity.IsPublished,
                SortOrder = sourceEntity.SortOrder,
                AppId = sourceEntity.AppId,
                ContentType = sourceEntity.ContentType,
                Json = sourceEntity.Json,
                Version = 1 // start over with numbering
            };

            DbContext.SqlDb.Add(clone);

            DbContext.Values.CloneEntitySimpleValues(sourceEntity, clone);

            return clone;
        }
        #endregion  

    }
}