using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Apps.AppSys;
using ToSic.Eav.Data.Build;

namespace ToSic.Eav.Apps.Parts
{
    public class EntityWorkCreate: AppWorkBase<IAppWorkCtxWithDb>
    {
        private readonly AppWork _appWork;
        private readonly DataBuilder _builder;

        public EntityWorkCreate(AppWork appWork, DataBuilder builder) : base("AWk.EntCre")
        {
            ConnectServices(
                _appWork = appWork,
                _builder = builder
            );
        }

        public (int EntityId, Guid EntityGuid) Create(string typeName, Dictionary<string, object> values, ITarget metadataFor = null) 
        {
            var l = Log.Fn<(int EntityId, Guid EntityGuid)>($"type:{typeName}, val-count:{values.Count}, meta:{metadataFor}");
            
            var newEnt = _builder.Entity.Create(appId: AppWorkCtx.AppState.AppId, guid: Guid.NewGuid(),
                contentType: AppWorkCtx.AppState.GetContentType(typeName),
                attributes: _builder.Attribute.Create(values),
                metadataFor: metadataFor);

            var eid = _appWork.EntitySave(AppWorkCtx).Save(newEnt);
            var guid = AppWorkCtx.DataController.Entities.TempLastSaveGuid;

            return l.Return((eid, guid), $"id:{eid}, guid:{guid}");
        }

        /// <summary>
        /// Get an entity, or create it with the values provided.
        /// Important for use cases, where an information must exist for sure, so it would be created with the provided defaults
        /// </summary>
        /// <param name="newGuid"></param>
        /// <param name="typeName"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public int GetOrCreate(Guid newGuid, string typeName, Dictionary<string, object> values)
        {
            Log.A($"get or create guid:{newGuid}, type:{typeName}, val-count:{values.Count}");
            if (AppWorkCtx.DataController.Entities.EntityExists(newGuid))
            {
                // check if it's deleted - if yes, resurrect
                var existingEnt = AppWorkCtx.DataController.Entities.GetEntitiesByGuid(newGuid).First();
                if (existingEnt.ChangeLogDeleted != null)
                    existingEnt.ChangeLogDeleted = null;

                return existingEnt.EntityId;
            }

            var appState = AppWorkCtx.AppState;
            var newE = _builder.Entity.Create(appId: AppWorkCtx.AppId, guid: newGuid,
                contentType: appState.GetContentType(typeName),
                attributes: _builder.Attribute.Create(values));
            return _appWork.EntitySave(AppWorkCtx).Save(newE);
        }

    }
}
