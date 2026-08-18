using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Metadata;
using ToSic.Eav.Metadata.Targets;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkMetadata(
    DataAssembler dataAssembler,
    AppWorkChain<WorkEntitySave> workEntSave,
    AppWorkChain<WorkEntityUpdate> entityUpdate)
    : ServiceWithSetup<IAppWorkContext>("AWk.EntMd", connect: [entityUpdate, dataAssembler, workEntSave])
{
    public void SaveMetadata(Target target, string typeName, Dictionary<string, object> values)
    {
        var l = Log.Fn($"target:{target.KeyNumber}/{target.KeyGuid}, {nameof(typeName)}: '{typeName}', values count:{values.Count}");
        if (target.TargetType != (int)TargetTypes.Attribute || target.KeyNumber == null || target.KeyNumber == 0)
            throw new NotSupportedException("atm this command only creates metadata for entities with id-keys");

        // see if a metadata already exists which we would update
        var existingEntity = MyOptions.AppReader.List
            .GetAll(typeName)
            .FirstOrDefault(e => e.MetadataFor.TargetType == target.TargetType && e.MetadataFor.KeyNumber == target.KeyNumber);

        if (existingEntity != null)
        {
            l.A($"Found and will update: {existingEntity.EntityId}");
            entityUpdate.New(MyOptions).UpdateParts(existingEntity.EntityId, values, new());
        }
        else
        {
            var appState = MyOptions.AppReader;
            var saveEnt = dataAssembler.Entity.Create(appId: MyOptions.AppId, guid: Guid.NewGuid(),
                contentType: appState.GetContentType(typeName),
                attributes: dataAssembler.AttributeList.Finalize(values!),
                metadataFor: target);

            var entSaver = workEntSave.New(MyOptions);
            entSaver.Save(saveEnt, entSaver.SaveOptions());
        }

        l.Done();
    }

}