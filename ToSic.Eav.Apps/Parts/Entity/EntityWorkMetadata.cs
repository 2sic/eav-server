using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Apps.AppSys;
using ToSic.Eav.Data.Build;

namespace ToSic.Eav.Apps.Parts
{
    public class EntityWorkMetadata : AppWorkBase<IAppWorkCtxWithDb>
    {
        private readonly AppWork _appWork;
        private readonly DataBuilder _builder;

        public EntityWorkMetadata(AppWork appWork, DataBuilder builder) : base("AWk.EntCre")
        {
            _appWork = appWork;
            _builder = builder;
        }

        public void SaveMetadata(Target target, string typeName, Dictionary<string, object> values) 
        {
            var l = Log.Fn($"target:{target.KeyNumber}/{target.KeyGuid}, values count:{values.Count}");
            if (target.TargetType != (int)TargetTypes.Attribute || target.KeyNumber == null || target.KeyNumber == 0)
                throw new NotSupportedException("atm this command only creates metadata for entities with id-keys");

            // see if a metadata already exists which we would update
            var existingEntity = AppWorkCtx.AppState.List
                .FirstOrDefault(e => e.MetadataFor?.TargetType == target.TargetType && e.MetadataFor?.KeyNumber == target.KeyNumber);
            if (existingEntity != null)
            {
                // #ExtractEntitySave - ???
                _appWork.EntityUpdate(AppWorkCtx)
                    .UpdateParts(existingEntity.EntityId, values);
            }
            else
            {
                var appState = AppWorkCtx.AppState;
                var saveEnt = _builder.Entity.Create(appId: AppWorkCtx.AppId, guid: Guid.NewGuid(),
                    contentType: appState.GetContentType(typeName),
                    attributes: _builder.Attribute.Create(values),
                    metadataFor: target);
                //saveEnt.SetMetadata(target);
                // #ExtractEntitySave - ???
                _appWork.EntitySave(AppWorkCtx)
                    .Save(saveEnt);
            }

            l.Done();
        }

    }
}
