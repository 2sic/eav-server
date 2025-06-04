using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.EntityPair.Sys;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.WebApi.Formats;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.WebApi.SaveHelpers;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class SaveEntities(EntityBuilder entityBuilder, GenWorkDb<WorkEntitySave> workEntSave)
    : ServiceBase("Eav.SavHlp", connect: [entityBuilder, workEntSave])
{
    public void UpdateGuidAndPublishedAndSaveMany(IAppWorkCtx appCtx, List<BundleWithHeader<IEntity>> itemsToImport, bool enforceDraft)
    {
        var l = Log.Fn($"will save {itemsToImport.Count} items");

        var saver = workEntSave.New(appCtx.AppReader);
        var saveOptions = saver.SaveOptions();

        var entitiesToImport = itemsToImport
            .Select(bundle => entityBuilder.CreateFrom(
                    bundle.Entity,
                    guid: bundle.Header.Guid,
                    isPublished: enforceDraft ? false : null
                )
            )
            .Select(e => new EntityPair<SaveOptions>(e, saveOptions with { DraftShouldBranch = enforceDraft }))
            .ToList();

        saver.Save(entitiesToImport);
        l.Done();
    }

    /// <summary>
    /// Generate pairs of guid/id of the newly added items
    /// </summary>
    /// <returns></returns>
    public Dictionary<Guid, int> GenerateIdList(WorkEntities workEntities, IEnumerable<BundleWithHeader> items)
    {
        var l = Log.Fn<Dictionary<Guid, int>>();
            
        var idList = items.Select(e =>
            {
                var foundEntity = workEntities.Get(e.Header.Guid);
                var state = foundEntity == null ? "not found" : foundEntity.IsPublished ? "published" : "draft";
                var draft = foundEntity  == null ? null : workEntities.AppWorkCtx.AppReader.GetDraft(foundEntity);
                l.A($"draft check: entity {e.Header.Guid} ({state}) - additional draft: {draft != null} - will return the draft");
                return draft ?? foundEntity; // return the draft (that would be the latest), or the found, or null if not found
            })
            .Where(e => e != null)
            .ToDictionary(f => f.EntityGuid, f => f.EntityId);
        return l.ReturnAsOk(idList);
    }

}