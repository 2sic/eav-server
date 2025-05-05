using ToSic.Eav.Data.Build;
using ToSic.Eav.Persistence;
using UpdateList = System.Collections.Generic.Dictionary<string, object>;

namespace ToSic.Eav.Apps.Internal.Work;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class WorkEntityUpdate(
    DataBuilder builder,
    LazySvc<EntitySaver> entitySaverLazy,
    GenWorkDb<WorkEntitySave> workEntSave)
    : WorkUnitBase<IAppWorkCtxWithDb>("AWk.EntUpd", connect: [builder, entitySaverLazy, workEntSave])
{
    /// <summary>
    /// Update an entity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="values"></param>
    /// <param name="publishing">Optionally specify that it should be a draft change</param>
    public void UpdateParts(int id, UpdateList values, EntitySavePublishing publishing) =>
        Log.Do($"id:{id}", () => UpdatePartsFromValues(AppWorkCtx.AppReader.List.FindRepoId(id), values, publishing));

    /// <summary>
    /// Update an entity
    /// </summary>
    /// <param name="id"></param>
    /// <param name="partialEntity"></param>
    /// <param name="publishing">specify that it should be a draft change</param>
    public void UpdateParts(int id, IEntity partialEntity, EntitySavePublishing publishing) =>
        Log.Do($"id:{id}", () => UpdatePartFromEntity(AppWorkCtx.AppReader.List.FindRepoId(id), partialEntity, publishing));


    /// <summary>
    /// Update an entity
    /// </summary>
    /// <param name="orig">Original entity to be updated</param>
    /// <param name="values">Dictionary of values to update</param>
    /// <param name="publishing">Optionally specify that it should be a draft change</param>
    internal bool UpdatePartsFromValues(IEntity orig, UpdateList values, EntitySavePublishing publishing)
    {
        var l = Log.Fn<bool>();
        var tempEnt = CreatePartialEntityOld(orig, values);
        if (tempEnt == null) return l.ReturnFalse("nothing to import");
        var result = UpdatePartFromEntity(orig, tempEnt, publishing);
        return l.ReturnTrue($"{result}");
    }


    /// <summary>
    /// Update an entity
    /// </summary>
    /// <param name="orig">Original entity to be updated</param>
    /// <param name="partialEntity">Partial Entity to update</param>
    /// <param name="publishing">Optionally specify that it should be a draft change</param>
    private bool UpdatePartFromEntity(IEntity orig, IEntity partialEntity, EntitySavePublishing publishing)
    {
        var l = Log.Fn<bool>();
        if (partialEntity == null)
            return l.ReturnFalse("nothing to import");

        var entSaver = workEntSave.New(AppWorkCtx);
        var saveOptions = entSaver.SaveOptions() with
        {
            PreserveUntouchedAttributes = true,
            PreserveUnknownLanguages = true,
            DraftShouldBranch = publishing?.ShouldBranchDrafts ?? false,
        };

        var saveEnt = entitySaverLazy.Value.CreateMergedForSaving(orig, partialEntity, saveOptions);

        entSaver.Save(saveEnt, saveOptions);
        return l.ReturnTrue("ok");
    }



    private Entity CreatePartialEntityOld(IEntity orig, UpdateList values)
    {
        var l = Log.Fn<Entity>();
        if (values == null || !values.Any())
            return l.ReturnNull("nothing to save");

        return l.Return(builder.Entity.Create(appId: AppWorkCtx.AppId, contentType: orig.Type, attributes: builder.Attribute.Create(values)), "ok");
    }
}