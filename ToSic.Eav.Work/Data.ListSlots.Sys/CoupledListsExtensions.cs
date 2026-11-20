namespace ToSic.Eav.Data.ListSlots.Sys;

/// <summary>
/// This is responsible for managing / changing list-pairs of entities.
/// These are usually relationship-properties of an entity,
/// like the Content items on a ContentBlock
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
internal static class CoupledListsExtensions
{
    public static Dictionary<string, List<int?>> Stabilize(this Dictionary<string, List<int?>> lists, ILog log)
    {
        var l = log.Fn<Dictionary<string, List<int?>>>();
        // Quick pre-validation - if there are no lists, something is wrong and we should break early
        if (lists.Count == 0)
            throw new("Lists Modifier needs at least 1 list, can't continue");

        // Determine how many values should be in each list, and then ensure that all lists have the same length.
        // This is important because we are working with coupled lists, and they need to be in sync.
        var expectedValuesCount = lists.First().Value.Count;
        l.A($"length: {expectedValuesCount}");

        // Sync lengths of values in the list
        // This does various clean-up, because historic data can often be mismatched.
        // For example, a list-pair of content/presentation may not be in correct state,
        // so this ensures that everything is in sync.
        var result = lists.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.SetLength(expectedValuesCount),
            StringComparer.InvariantCultureIgnoreCase);

        return l.Return(result, $"ok, length of each element: {expectedValuesCount}");
    }

    /// <summary>
    /// If SortOrder is not specified, adds at the end
    /// </summary>
    /// <param name="lists"></param>
    /// <param name="index">Index to add to - 0 based</param>
    /// <param name="ids"></param>
    /// <param name="log"></param>
    public static Dictionary<string, List<int?>> AddSlot(this Dictionary<string, List<int?>> lists, int? index, int?[] ids, ILog? log)
    {
        var l = log.Fn<Dictionary<string, List<int?>>>($"add content/pres at order:{index}");
        var max = lists.First().Value.Count; // max is at the end of the list
        index ??= max;
        if (index > max)
            index = max;
        if (index < 0)
            index = 0;
        var i = 0;
        lists.ForEach(slot => slot.InsertOrAppend(index.Value, ids[i++]));
        return l.Return(lists, "ok");
    }

    /// <summary>
    /// Move an item in the coupled lists
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, List<int?>> MoveSlot(this Dictionary<string, List<int?>> lists, int sourceIndex, int targetIndex, ILog? log)
    {
        var l = log.Fn<Dictionary<string, List<int?>>>($"reorder entities before:{sourceIndex} to after:{targetIndex}");
        var hasChanges = lists.Values
            .Aggregate(false, (prev, lstItem) => lstItem.Move(sourceIndex, targetIndex) || prev);
        return hasChanges
            ? l.Return(lists, "ok")
            : l.Return([],"outside of range, no changes");
    }


    /// <summary>
    /// Reorder the pair of sequences
    /// </summary>
    /// <param name="original"></param>
    /// <param name="newSequence">List of index-IDs how it should be sorted now</param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static Dictionary<string, List<int?>> ReorderSlots(this Dictionary<string, List<int?>> original, int[] newSequence, ILog? log)
    {
        var l = log.Fn<Dictionary<string, List<int?>>>($"seq:[{string.Join(",", newSequence)}]");
        // some error checks
        if (newSequence.Length != original.First().Value.Count)
        {
            const string msg = "Error: Can't re-order - list length is different";
            throw l.Done(new Exception(msg));
        }

        const int usedMarker = int.MinValue;
        original.ForEach(lst =>
        {
            var copy = lst.ToListOpt();
            lst.Clear();
            foreach (var index in newSequence)
            {
                if (copy[index] == usedMarker)
                    throw new($"Error: Cancelled re-order because index {index} was re-used");
                var primaryId = copy[index];
                copy[index] = usedMarker;
                lst.Add(primaryId);
                l.A($"Added at [{index}] value {primaryId}");
            }
        });

        return l.Return(original, "ok");
    }


    /// <summary>
    /// Replace an item in the primary list, and optionally also in the coupled list. 
    /// </summary>
    /// <param name="original"></param>
    /// <param name="index"></param>
    /// <param name="values"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    public static Dictionary<string, List<int?>> ReplaceSlot(this Dictionary<string, List<int?>> original, int index, (bool MayReplace, int? Values)[] values, ILog? log)
    {
        var l = log.Fn<Dictionary<string, List<int?>>>($"index: {index}");
        if (index == -1)
            throw l.Ex(new Exception("Sort order is never -1 any more; deprecated"));

        // if necessary, increase length on all
        var i = 0;
        var ok = original.Values.Aggregate(false, (hasChanges, lst) =>
        {
            lst.EnsureListLength(index + 1);
            var configPair = values[i++];
            if (!configPair.MayReplace)
                return hasChanges;
            var (thisHasChanged, _) = ReplaceItemAtIndexIfChanged(lst, index, configPair.Values);
            return hasChanges || thisHasChanged;
        });

        return ok
            ? l.Return(original)
            : l.Return([]);

        // Helper, in future should become fully functional, ATM not given
        static (bool HasChanged, List<int?> List) ReplaceItemAtIndexIfChanged(List<int?> listMain, int index, int? entityId)
        {
            if (listMain[index] == entityId)
                return (false, listMain);
            listMain[index] = entityId;
            return (true, listMain);
        }
    }

    /// <summary>
    /// Removes entries from the coupled pair.
    /// </summary>
    /// <param name="original">The original dictionary of lists</param>
    /// <param name="index">Index to remove, 0 based</param>
    /// <param name="log">Optional log instance</param>
    public static Dictionary<string, List<int?>> RemoveSlots(this Dictionary<string, List<int?>> original, int index, ILog? log)
    {
        var l = log.Fn<Dictionary<string, List<int?>>>($"remove content and pres items order:{index}");
        original.ForEach(slot => slot.RemoveIfInRange(index));
        return l.Return(original, "ok");
    }


}
