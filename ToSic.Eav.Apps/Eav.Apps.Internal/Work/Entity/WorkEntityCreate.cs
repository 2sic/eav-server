using ToSic.Eav.Data.Build;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps.Internal.Work;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class WorkEntityCreate(DataBuilder builder, GenWorkDb<WorkEntitySave> workEntSave)
    : WorkUnitBase<IAppWorkCtxWithDb>("AWk.EntCre", connect: [workEntSave, builder])
{
    public (int EntityId, Guid EntityGuid) Create(string typeName, Dictionary<string, object> values, ITarget metadataFor = null)
    {
        var l = Log.Fn<(int EntityId, Guid EntityGuid)>($"type:{typeName}, val-count:{values.Count}, meta:{metadataFor}");

        var newEnt = builder.Entity.Create(appId: AppWorkCtx.AppId, guid: Guid.NewGuid(),
            contentType: AppWorkCtx.AppReader.GetContentType(typeName),
            attributes: builder.Attribute.Create(values),
            metadataFor: metadataFor);

        var entSaver = workEntSave.New(AppWorkCtx);
        var eid = entSaver.Save(newEnt, entSaver.SaveOptions());
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
            // 2025-04-29 2dm code changed a bit for 2sxc v20.0, but couldn't see how to test it / when this scenario pops up
            // remove this comment if everything is ok by 2026-04
            var existingEnt = AppWorkCtx.DataController.Entities.GetEntityStubsByGuid(newGuid).First();
            if (existingEnt.TransactionIdDeleted != null)
                existingEnt.TransactionIdDeleted = null;

            return existingEnt.EntityId;
        }

        var newE = builder.Entity.Create(appId: AppWorkCtx.AppId, guid: newGuid,
            contentType: AppWorkCtx.AppReader.GetContentType(typeName),
            attributes: builder.Attribute.Create(values));
        var entSaver = workEntSave.New(AppWorkCtx);
        return entSaver.Save(newE, entSaver.SaveOptions());
    }

}