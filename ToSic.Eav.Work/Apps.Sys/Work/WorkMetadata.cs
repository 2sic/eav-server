using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Targets;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkMetadata(
    DataAssembler dataAssembler,
    GenWorkDb<WorkEntitySave> workEntSave,
    GenWorkDb<WorkEntityUpdate> entityUpdate)
    : WorkUnitBase<IAppWorkCtxWithDb>("AWk.EntMd", connect: [entityUpdate, dataAssembler, workEntSave])
{
    public void SaveMetadata(Target target, string typeName, Dictionary<string, object> values)
    {
        var l = Log.Fn($"target:{target.KeyNumber}/{target.KeyGuid}, {nameof(typeName)}: '{typeName}', values count:{values.Count}");
        if (target.TargetType != (int)TargetTypes.Attribute || target.KeyNumber == null || target.KeyNumber == 0)
            throw new NotSupportedException("atm this command only creates metadata for entities with id-keys");

        // see if a metadata already exists which we would update
        var existingEntity = AppWorkCtx.AppReader.List
            .GetAll(typeName)
            .FirstOrDefault(e => e.MetadataFor.TargetType == target.TargetType && e.MetadataFor.KeyNumber == target.KeyNumber);

        if (existingEntity != null)
        {
            l.A($"Found and will update: {existingEntity.EntityId}");
            entityUpdate.New(AppWorkCtx).UpdateParts(existingEntity.EntityId, values, new());
        }
        else
        {
            var appState = AppWorkCtx.AppReader;
            var saveEnt = dataAssembler.Entity.Create(appId: AppWorkCtx.AppId, guid: Guid.NewGuid(),
                contentType: appState.GetContentType(typeName),
                attributes: dataAssembler.AttributeList.Finalize(values!),
                metadataFor: target);

            var entSaver = workEntSave.New(AppWorkCtx);
            entSaver.Save(saveEnt, entSaver.SaveOptions());
        }

        l.Done();
    }

}