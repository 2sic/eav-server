using ToSic.Eav.Data.Build;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps.Internal.Work;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class WorkMetadata(
    DataBuilder builder,
    GenWorkDb<WorkEntitySave> workEntSave,
    GenWorkDb<WorkEntityUpdate> entityUpdate)
    : WorkUnitBase<IAppWorkCtxWithDb>("AWk.EntMd", connect: [entityUpdate, builder, workEntSave])
{
    public void SaveMetadata(Target target, string typeName, Dictionary<string, object> values)
    {
        var l = Log.Fn($"target:{target.KeyNumber}/{target.KeyGuid}, values count:{values.Count}");
        if (target.TargetType != (int)TargetTypes.Attribute || target.KeyNumber == null || target.KeyNumber == 0)
            throw new NotSupportedException("atm this command only creates metadata for entities with id-keys");

        // see if a metadata already exists which we would update
        var existingEntity = AppWorkCtx.AppState.List
            .FirstOrDefault(e => e.MetadataFor?.TargetType == target.TargetType && e.MetadataFor?.KeyNumber == target.KeyNumber);
        if (existingEntity != null)
            entityUpdate.New(AppWorkCtx).UpdateParts(existingEntity.EntityId, values);
        else
        {
            var appState = AppWorkCtx.AppState;
            var saveEnt = builder.Entity.Create(appId: AppWorkCtx.AppId, guid: Guid.NewGuid(),
                contentType: appState.GetContentType(typeName),
                attributes: builder.Attribute.Create(values),
                metadataFor: target);
            workEntSave.New(AppWorkCtx).Save(saveEnt);
        }

        l.Done();
    }

}