﻿namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// This is responsible for managing / changing list-pairs of entities.
/// These are usually relationship-properties of an entity,
/// like the Content items on a ContentBlock
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class CoupledIdLists: HelperBase
{
    public Dictionary<string, List<int?>> Lists = new(StringComparer.InvariantCultureIgnoreCase);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="lists">Lists to use for working on</param>
    /// <param name="parentLog"></param>
    public CoupledIdLists(Dictionary<string, List<int?>> lists, ILog parentLog) : base(parentLog, "App.LstPai")
    {
        foreach (var keyValuePair in lists) Lists.Add(keyValuePair.Key, keyValuePair.Value);
        SyncListLengths();

        if(Lists.Count == 0)
            throw new("Lists Modifier needs at least 1 list, can't continue");
    }

    /// <summary>
    /// If SortOrder is not specified, adds at the end
    /// </summary>
    /// <param name="index">Index to add to - 0 based</param>
    /// <param name="ids"></param>
    public Dictionary<string, object?> Add(int? index, int?[] ids)
    {
        Log.A($"add content/pres at order:{index}");
        var max = Lists.First().Value.Count; // max is at the end of the list
        index = index ?? max;
        if (index > max)
            index = max;
        if (index < 0)
            index = 0;
        var i = 0;
        Lists.ForEach(l => l.InsertOrAppend(index.Value, ids[i++]));
        return Lists.ToObject();
    }

    /// <summary>
    /// Removes entries from the coupled pair.
    /// </summary>
    /// <param name="index">Index to remove, 0 based</param>
    public Dictionary<string, object?> Remove(int index)
    {
        Log.A($"remove content and pres items order:{index}");
        Lists.ForEach(l => l.RemoveIfInRange(index));
        return Lists.ToObject();
    }

    /// <summary>
    /// Move an item in the coupled lists
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, object?> Move(int sourceIndex, int targetIndex)
    {
        var l = Log.Fn<Dictionary<string, object?>>($"reorder entities before:{sourceIndex} to after:{targetIndex}");
        var hasChanges = Lists.Values
            .Aggregate(false, (prev, lstItem) => lstItem.Move(sourceIndex, targetIndex) || prev);
        return hasChanges
            ? l.Return(Lists.ToObject(), "ok")
            : l.Return([],"outside of range, no changes");
    }

    /// <summary>
    /// Reorder the pair of sequences
    /// </summary>
    /// <param name="newSequence">List of index-IDs how it should be sorted now</param>
    /// <returns></returns>
    public Dictionary<string, object?> Reorder(int[] newSequence)
    {
        var l = Log.Fn<Dictionary<string, object?>>($"seq:[{string.Join(",", newSequence)}]");
        // some error checks
        if (newSequence.Length != Lists.First().Value.Count)
        {
            const string msg = "Error: Can't re-order - list length is different";
            throw l.Done(new Exception(msg));
        }

        const int usedMarker = int.MinValue;
        Lists.ForEach(lst =>
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

        return l.Return(Lists.ToObject(), "ok");
    }

    /// <summary>
    /// Replace an item in the primary list, and optionally also in the coupled list. 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public Dictionary<string, object?> Replace(int index, (bool, int?)[] values)
    {
        var l = Log.Fn<Dictionary<string, object?>>($"index: {index}");
        if (index == -1)
            throw l.Ex(new Exception("Sort order is never -1 any more; deprecated"));

        // if necessary, increase length on all
        var i = 0;
        var ok = Lists.Values.Aggregate(false, (prev, lst) =>
        {
            lst.EnsureListLength(index + 1);
            var set = values[i++];
            var changed = set.Item1 && ReplaceItemAtIndexIfChanged(lst, index, set.Item2);
            return prev || changed;
        });

        return ok
            ? l.Return(Lists.ToObject())
            : l.Return([]);
    }

    private static bool ReplaceItemAtIndexIfChanged(List<int?> listMain, int index, int? entityId)
    {
        if (listMain[index] == entityId)
            return false;
        listMain[index] = entityId;
        return true;
    }

    /// <summary>
    /// This method does various clean-up, because historic data can often be mismatched.
    /// For example, a list-pair of content/presentation may not be in correct state, so this ensures that everything
    /// is in sync.
    /// </summary>
    /// <returns></returns>
    private void SyncListLengths()
    {
        var expectedLength = Lists.First().Value.Count;
        Log.Do($"length: {expectedLength}", () => Lists.Skip(1).ForEach(l => l.SetLength(expectedLength)));
    }
}