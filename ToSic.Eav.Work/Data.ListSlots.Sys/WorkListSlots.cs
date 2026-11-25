using ToSic.Eav.Apps.Sys.State;

namespace ToSic.Eav.Data.ListSlots.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkListSlots(AppWorkChain<WorkEntityUpdate> entityUpdate)
    : ServiceWithSetup<IAppWorkContext>("AWk.EntFL", connect: [entityUpdate])
{
    public record Parameters(IEntity Target, string[] Fields, bool AsDraft, Func<Dictionary<string, List<int?>>, Dictionary<string, List<int?>>> Modify);

    public void Process(Parameters parameters)
    {
        var (target, fields, asDraft, modify) = parameters;

        // Get the best entity - live or draft.
        target = MyOptions.AppReader.GetDraftOrKeep(target);

        // Make sure we have a clean setup before calling the modification callback
        var listsWithIds = fields.ToDictionary(
            f => f,
            f => SlotIdsWithNulls(target.Children(f)));
        var lists = listsWithIds.Stabilize(Log);

        // Process and convert to string/object dictionary as that is expected
        var values = modify.Invoke(lists).ToObject();

        // Save
        entityUpdate.New(MyOptions)
            .UpdatePartsFromValues(target, values, new() { ShouldPublish = !asDraft, ShouldBranchDrafts = asDraft });
        return;

        static List<int?> SlotIdsWithNulls(IEnumerable<IEntity?> list) => [.. list.Select(p => p?.EntityId)];
    }

    public void AddSlot(IEntity target, string[] fields, int index, int?[] values, bool asDraft, bool forceAddToEnd, bool padWithNulls = false)
        => Process(new(target, fields, asDraft, lists =>
        {
            // hitting + if the list is empty first add 1 null item (because we already see one demo item)
            // fix https://github.com/2sic/2sxc/issues/2943 
            // on empty and padWithNulls, first add 1 null item
            if (lists.First().Value.Count < index && padWithNulls)
                lists.AddSlot(0, [null, null], Log);

            return lists.AddSlot(forceAddToEnd ? null : index, values, Log);
        }));



    public void RemoveSlot(IEntity target, string[] fields, int index, bool asDraft)
        => Process(new(target, fields, asDraft, lists => lists.RemoveSlots(index, Log)));

    public void MoveSlot(IEntity target, string[] fields, int source, int dest, bool asDraft)
        => Process(new(target, fields, asDraft, lists => lists.MoveSlot(source, dest, Log)));

    public void ReorderSlots(IEntity target, string[] fields, int[] newSequence, bool asDraft)
        => Process(new(target, fields, asDraft, lists => lists.ReorderSlots(newSequence, Log)));


    public void ReplaceSlot(IEntity target, string[] fields, int index, int?[] replacement, bool asDraft)
        => Process(new(target, fields, asDraft, lists => lists
            .ReplaceSlot(index, [.. replacement.Select(r => (true, r))], Log)));
}
