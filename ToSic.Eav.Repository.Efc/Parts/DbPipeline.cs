using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public class DbPipeline: BllCommandBase
    {
        public DbPipeline(DbDataController c) : base(c) { }

        /// <summary>
        /// Copy an existing DataPipeline by copying all Entities and uptdate their GUIDs
        /// </summary>
        public int CopyDataPipeline(int appId, int pipelineEntityId, string userName)
        {
            DbContext.UserName = userName;

            // Clone Pipeline Entity with a new new Guid
            string importantIncludes = "AttributeSet,ToSicEavValues,ToSicEavValues.ToSicEavValuesDimensions";
            var sourcePipelineEntity = DbContext.Entities.GetDbEntity(pipelineEntityId, importantIncludes + ",ToSicEavValues.Attribute");
            if (sourcePipelineEntity.AttributeSet.StaticName != Constants.DataPipelineStaticName) //PipelineAttributeSetStaticName)
                throw new ArgumentException("Entity is not an DataPipeline Entity", nameof(pipelineEntityId));
            var pipelineEntityClone = DbContext.Entities.CloneEntity(sourcePipelineEntity);
            
            // Copy Pipeline Parts with configuration Entity, assign KeyGuid of the new Pipeline Entity
            var pipelineParts = DbContext.Entities.GetEntityMetadataByGuid(appId, sourcePipelineEntity.EntityGuid, includes: importantIncludes);
            var pipelinePartClones = new Dictionary<string, Guid>();	// track Guids of originals and their clone
            foreach (var pipelinePart in pipelineParts)
            {
                var pipelinePartClone = DbContext.Entities.CloneEntity(pipelinePart);
                pipelinePartClone.KeyGuid = pipelineEntityClone.EntityGuid;
                pipelinePartClones.Add(pipelinePart.EntityGuid.ToString(), pipelinePartClone.EntityGuid);

                // Copy Configuration Entity, assign KeyGuid of the Clone
                var configurationEntity = DbContext.Entities.GetEntityMetadataByGuid(appId, pipelinePart.EntityGuid, includes: importantIncludes).SingleOrDefault();
                if (configurationEntity != null)
                {
                    var configurationClone = DbContext.Entities.CloneEntity(configurationEntity);
                    configurationClone.KeyGuid = pipelinePartClone.EntityGuid;
                }
            }

            #region Update Stream-Wirings

            // need the wiring attribute id for later, as I can't detect it on the unsaved clone
            var wiringAttributeId = sourcePipelineEntity.ToSicEavValues.Single(v => v.Attribute.StaticName == Constants.DataPipelineStreamWiringStaticName).AttributeId;

            var streamWiring = pipelineEntityClone.ToSicEavValues.Single(v => v.AttributeId == wiringAttributeId /*v.Attribute.StaticName == Constants.DataPipelineStreamWiringStaticName*/);// StreamWiringAttributeName);
            var wiringsClone = new List<WireInfo>();
            var wiringsSource = DataPipelineWiring.Deserialize(streamWiring.Value);
            if (wiringsSource != null)
            {
                foreach (var wireInfo in wiringsSource)
                {
                    var wireInfoClone = wireInfo; // creates a clone of the Struct
                    if (pipelinePartClones.ContainsKey(wireInfo.From))
                        wireInfoClone.From = pipelinePartClones[wireInfo.From].ToString();
                    if (pipelinePartClones.ContainsKey(wireInfo.To))
                        wireInfoClone.To = pipelinePartClones[wireInfo.To].ToString();

                    wiringsClone.Add(wireInfoClone);
                }
            }

            streamWiring.Value = DataPipelineWiring.Serialize(wiringsClone);
            #endregion

            DbContext.SqlDb.SaveChanges();

            return pipelineEntityClone.EntityId;
        }
    }
}