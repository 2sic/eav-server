using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Data.Build;

namespace ToSic.Eav.Apps.Work;

public class WorkMetadata : WorkUnitBase<IAppWorkCtxWithDb>
{
    private readonly GenWorkDb<WorkEntityUpdate> _entityUpdate;
    private readonly GenWorkDb<WorkEntitySave> _workEntSave;
    private readonly DataBuilder _builder;

    public WorkMetadata(DataBuilder builder, GenWorkDb<WorkEntitySave> workEntSave, GenWorkDb<WorkEntityUpdate> entityUpdate) : base("AWk.EntMd")
    {
        ConnectServices(
            _entityUpdate = entityUpdate,
            _builder = builder,
            _workEntSave = workEntSave
        );
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
            _entityUpdate.New(AppWorkCtx).UpdateParts(existingEntity.EntityId, values);
        else
        {
            var appState = AppWorkCtx.AppState;
            var saveEnt = _builder.Entity.Create(appId: AppWorkCtx.AppId, guid: Guid.NewGuid(),
                contentType: appState.GetContentType(typeName),
                attributes: _builder.Attribute.Create(values),
                metadataFor: target);
            _workEntSave.New(AppWorkCtx).Save(saveEnt);
        }

        l.Done();
    }

}