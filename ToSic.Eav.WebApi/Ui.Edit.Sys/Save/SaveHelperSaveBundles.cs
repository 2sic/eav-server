using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.Ui.Edit.Sys.Save;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class SaveHelperSaveBundles(EntityAssembler entityAssembler, AppWorkChain<WorkEntitySave> workEntSave)
    : ServiceWithSetup<IAppWorkContext>("Eav.SavHlp", connect: [entityAssembler, workEntSave])
{
    public void UpdateGuidAndPublishedAndSaveMany(IAppWorkContext appCtx, List<BundleWithHeader<IEntity>> itemsToImport, bool enforceDraft)
    {
        var l = Log.Fn($"will save {itemsToImport.Count} items");

        var saver = workEntSave.New(appCtx);
        var saveOptions = saver.SaveOptions();

        var entitiesToImport = itemsToImport
            .Select(bundle => entityAssembler.CreateFrom(
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
}
