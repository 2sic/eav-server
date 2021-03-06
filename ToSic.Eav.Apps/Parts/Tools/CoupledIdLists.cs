﻿using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Logging;
using DicNameInt = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int?>>;
using DicNameObj = System.Collections.Generic.Dictionary<string, object>;

namespace ToSic.Eav.Apps.Parts.Tools
{
    /// <summary>
    /// This is responsible for managing / changing list-pairs of entities.
    /// These are usually relationship-properties of an entity,
    /// like the Content items on a ContentBlock
    /// </summary>
    public class CoupledIdLists: HasLog
    {
        public DicNameInt Lists = new DicNameInt(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lists">Lists to use for working on</param>
        /// <param name="parentLog">Logger</param>
        public CoupledIdLists(DicNameInt lists, ILog parentLog)
            : base("App.LstPai", parentLog)
        {
            foreach (var keyValuePair in lists) Lists.Add(keyValuePair.Key, keyValuePair.Value);
            SyncListLengths();

            if(Lists.Count == 0)
                throw new Exception("Lists Modifier needs at least 1 list, can't continue");
        }

        /// <summary>
        /// If SortOrder is not specified, adds at the end
        /// </summary>
        /// <param name="index">Index to add to - 0 based</param>
        /// <param name="ids"></param>
        public DicNameObj Add(int? index, int?[] ids)
        {
            Log.Add($"add content/pres at order:{index}");
            index = index ?? Lists.First().Value.Count;
            var i = 0;
            Lists.ForEach(l => l.InsertOrAppend(index.Value, ids[i++]));
            return Lists.ToObject();
        }

        /// <summary>
        /// Removes entries from the coupled pair.
        /// </summary>
        /// <param name="index">Index to remove, 0 based</param>
        public DicNameObj Remove(int index)
        {
            Log.Add($"remove content and pres items order:{index}");
            Lists.ForEach(l => l.RemoveIfInRange(index));
            return Lists.ToObject();
        }

        /// <summary>
        /// Move an item in the coupled lists
        /// </summary>
        /// <returns></returns>
        public DicNameObj Move(int sourceIndex, int targetIndex)
        {
            var wrapLog = Log.Call<DicNameObj>($"reorder entities before:{sourceIndex} to after:{targetIndex}");
            var hasChanges = Lists.Values
                .Aggregate(false, (prev, l) => l.Move(sourceIndex, targetIndex) || prev);
            return hasChanges
                ? wrapLog("ok", Lists.ToObject())
                : wrapLog("outside of range, no changes", null);
        }

        /// <summary>
        /// Reorder the pair of sequences
        /// </summary>
        /// <param name="newSequence">List of index-IDs how it should be sorted now</param>
        /// <returns></returns>
        public DicNameObj Reorder(int[] newSequence)
        {
            var wrapLog = Log.Call<DicNameObj>($"seq:[{string.Join(",", newSequence)}]");

            // some error checks
            if (newSequence.Length != Lists.First().Value.Count)
            {
                const string msg = "Error: Can't re-order - list length is different";
                wrapLog(msg, null);
                throw new Exception(msg);
            }

            const int usedMarker = int.MinValue;
            Lists.ForEach(l =>
            {
                var copy = l.ToList();
                l.Clear();
                foreach (var index in newSequence)
                {
                    if (copy[index] == usedMarker)
                        throw new Exception($"Error: Cancelled re-order because index {index} was re-used");
                    var primaryId = copy[index];
                    copy[index] = usedMarker;
                    l.Add(primaryId);
                    Log.Add($"Added at [{index}] value {primaryId}");
                }
            });

            return wrapLog("ok", Lists.ToObject());
        }

        /// <summary>
        /// Replace an item in the primary list, and optionally also in the coupled list. 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public DicNameObj Replace(int index, Tuple<bool, int?>[] values)
        {
            Log.Call($"index: {index}")(null);
            if (index == -1)
                throw new Exception("Sort order is never -1 any more; deprecated");

            // if necessary, increase length on all
            var i = 0;
            var ok = Lists.Values.Aggregate(false, (prev, l) =>
            {
                l.EnsureListLength(index + 1);
                var set = values[i++];
                var changed = set.Item1 && ReplaceItemAtIndexIfChanged(l, index, set.Item2);
                return prev || changed;
            });

            return ok ? Lists.ToObject() : null;
        }

        private static bool ReplaceItemAtIndexIfChanged(List<int?> listMain, int index, int? entityId)
        {
            if (listMain[index] == entityId) return false;
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
            var wrapLog = Log.Call($"length: {expectedLength}");
            Lists.Skip(1).ForEach(l => l.SetLength(expectedLength));
            wrapLog("ok");
        }
    }
}
