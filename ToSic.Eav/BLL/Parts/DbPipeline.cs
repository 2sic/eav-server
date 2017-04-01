using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.BLL.Parts
{
    public class DbPipeline: BllCommandBase
    {
        public DbPipeline(EavDataController c) : base(c) { }

        /// <summary>
        /// Copy an existing DataPipeline by copying all Entities and uptdate their GUIDs
        /// </summary>
        public Entity CopyDataPipeline(int appId, int pipelineEntityId, string userName)
        {
            Context.UserName = userName;

            // Clone Pipeline Entity with a new new Guid
            var sourcePipelineEntity = Context.Entities.GetEntity(pipelineEntityId);
            if (sourcePipelineEntity.Set.StaticName != Constants.DataPipelineStaticName) //PipelineAttributeSetStaticName)
                throw new ArgumentException("Entity is not an DataPipeline Entity", nameof(pipelineEntityId));
            var pipelineEntityClone = Context.Entities.CloneEntity(sourcePipelineEntity, true);

            // Copy Pipeline Parts with configuration Entity, assign KeyGuid of the new Pipeline Entity
            var pipelineParts = Context.Entities.GetEntities(Constants.AssignmentObjectTypeEntity, sourcePipelineEntity.EntityGUID);
            var pipelinePartClones = new Dictionary<string, Guid>();	// track Guids of originals and their clone
            foreach (var pipelinePart in pipelineParts)
            {
                var pipelinePartClone = Context.Entities.CloneEntity(pipelinePart, true);
                pipelinePartClone.KeyGuid = pipelineEntityClone.EntityGUID;
                pipelinePartClones.Add(pipelinePart.EntityGUID.ToString(), pipelinePartClone.EntityGUID);

                // Copy Configuration Entity, assign KeyGuid of the Clone
                var configurationEntity = Context.Entities.GetEntities(Constants.AssignmentObjectTypeEntity, pipelinePart.EntityGUID).SingleOrDefault();
                if (configurationEntity != null)
                {
                    var configurationClone = Context.Entities.CloneEntity(configurationEntity, true);
                    configurationClone.KeyGuid = pipelinePartClone.EntityGUID;
                }
            }

            #region Update Stream-Wirings

            var streamWiring = pipelineEntityClone.Values.Single(v => v.Attribute.StaticName == Constants.DataPipelineStreamWiringStaticName);// StreamWiringAttributeName);
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

            Context.SqlDb.SaveChanges();

            return pipelineEntityClone;
        }
    }
}