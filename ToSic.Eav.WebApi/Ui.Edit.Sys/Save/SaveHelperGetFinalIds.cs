using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.Ui.Edit.Sys.Save;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class SaveHelperGetFinalIds(AppDataChain<WorkEntities> workEntities)
    : ServiceWithSetup<IAppDataContext>("Eav.SavHlp", connect: [])
{
    /// <summary>
    /// Generate pairs of guid/id of the newly added items
    /// </summary>
    /// <returns></returns>
    public Dictionary<Guid, int> GenerateIdList(IEnumerable<BundleWithHeader> items)
    {
        var l = Log.Fn<Dictionary<Guid, int>>();

        var idList = items
            .Select(e =>
            {
                var foundEntity = workEntities.New(MyOptions).Get(e.Header.Guid);

                var state = foundEntity == null
                    ? "not found"
                    : foundEntity.IsPublished
                        ? "published"
                        : "draft";
                var draft = foundEntity == null
                    ? null
                    : MyOptions.AppReader.GetDraft(foundEntity);

                l.A($"draft check: entity {e.Header.Guid} ({state}) - additional draft: {draft != null} - will return the draft");
                
                // return the draft (that would be the latest), or the found, or null if not found
                return draft ?? foundEntity;
            })
            .Where(e => e != null)
            .ToDictionary(e => e!.EntityGuid, e => e!.EntityId);

        return l.ReturnAsOk(idList);
    }
}
