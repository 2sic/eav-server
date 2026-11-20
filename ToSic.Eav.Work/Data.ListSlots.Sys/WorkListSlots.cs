using ToSic.Eav.Apps.Sys.State;

namespace ToSic.Eav.Data.ListSlots.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkListSlots(AppWorkChain<WorkEntityUpdate> entityUpdate)
    : ServiceWithSetup<IAppWorkContext>("AWk.EntFL", connect: [entityUpdate])
{
    public void FieldListUpdate(IEntity target, string[] fields, bool asDraft, Func<Dictionary<string, List<int?>>, Dictionary<string, List<int?>>> callback)
    {
        target = MyOptions.AppReader.GetDraftOrKeep(target)!;
        var listsWithIds = fields.ToDictionary(
            f => f,
            f => SlotIdsWithNulls(target.Children(f)));
        var lists = listsWithIds.Stabilize(Log);
        var values = callback.Invoke(lists).ToObject();
        entityUpdate.New(MyOptions)
            .UpdatePartsFromValues(target, values, new() { ShouldPublish = !asDraft, ShouldBranchDrafts = asDraft});
        return;

        static List<int?> SlotIdsWithNulls(IEnumerable<IEntity?> list) => [.. list.Select(p => p?.EntityId)];
    }

    public void AddSlot(IEntity target, string[] fields, int index, int?[] values, bool asDraft, bool forceAddToEnd, bool padWithNulls = false)
        => FieldListUpdate(target, fields, asDraft, lists =>
        {
            // hitting + if the list is empty first add 1 null item (because we already see one demo item)
            // fix https://github.com/2sic/2sxc/issues/2943 
            if (lists.First().Value.Count < index && padWithNulls) // on non, first add 1 null item
                lists.AddSlot(0, [null, null], Log);

            return lists.AddSlot(forceAddToEnd ? null : (int?)index, values, Log);
        });



    public void RemoveSlot(IEntity target, string[] fields, int index, bool asDraft)
        => FieldListUpdate(target, fields, asDraft, lists => lists.RemoveSlots(index, Log));

    public void MoveSlot(IEntity target, string[] fields, int source, int dest, bool asDraft)
        => FieldListUpdate(target, fields, asDraft, lists => lists.MoveSlot(source, dest, Log));

    public void ReorderSlots(IEntity target, string[] fields, int[] newSequence, bool asDraft)
        => FieldListUpdate(target, fields, asDraft, lists => lists.ReorderSlots(newSequence, Log));


    public void ReplaceSlot(IEntity target, string[] fields, int index, int?[] replacement, bool asDraft)
        => FieldListUpdate(target, fields, asDraft,
            lists => lists.ReplaceSlot(index, [.. replacement.Select(r => (true, r))],
                Log));
}
