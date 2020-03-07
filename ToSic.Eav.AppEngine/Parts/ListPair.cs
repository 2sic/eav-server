using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Logging;
using UpdateList = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int?>>;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// This is responsible for managing / changing lists of entities.
    /// These are usually relationship-properties of an entity, like the Content items on a ContentBlock
    /// </summary>
    public class ListPair :HasLog
    {
        public string PrimaryField;
        public string CoupledField;

        internal List<int?> PrimaryIds;
        internal List<int?> CoupledIds; 


        public ListPair(List<int?> primary, List<int?> coupled, string primaryField, string coupledField,
            ILog parentLog)
            : base("Tmp.LstMng", parentLog)
        {
            PrimaryField = primaryField;
            CoupledField = coupledField;
            PrimaryIds = primary;
            CoupledIds = SyncCoupledListLength(PrimaryIds, coupled);
        }

        /// <summary>
        /// If SortOrder is not specified, adds at the end
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <param name="primaryId"></param>
        /// <param name="coupledId"></param>
        public UpdateList Add(int? sortOrder, int? primaryId, int? coupledId)
        {
            Log.Add($"add content/pres at order:{sortOrder}, content#{primaryId}, pres#{coupledId}");

            if (!sortOrder.HasValue)
                sortOrder = PrimaryIds.Count;

            PrimaryIds.InsertOrAppend(sortOrder.Value, primaryId);
            CoupledIds.InsertOrAppend(sortOrder.Value, coupledId);

            return BuildChangesList(PrimaryIds, CoupledIds);
        }

        /// <summary>
        /// Removes entities from a group. This will also remove the corresponding presentation entities.
        /// </summary>
        /// <param name="index"></param>
        public UpdateList Remove(int index)
        {
            Log.Add($"remove content and pres items type:{PrimaryField}, order:{index}");
            // always check if the length actually matches this, otherwise ignore
            if (PrimaryIds.Count > index)
                PrimaryIds.RemoveAt(index);
            // in many cases the presentation-list is empty, then there is nothing to remove
            if (CoupledIds.Count > index)
                CoupledIds.RemoveAt(index);
            return BuildChangesList(PrimaryIds, CoupledIds);
        }


        public UpdateList Move(int origin, int target)
        {
            var wrapLog = Log.Call<UpdateList>($"reorder entities before:{origin} to after:{target}");
            if (origin >= PrimaryIds.Count) 
                return wrapLog("outside of range, no changes", null);

            var contentId = PrimaryIds[origin];
            var presentationId = CoupledIds[origin];

            PrimaryIds.RemoveAt(origin);
            CoupledIds.RemoveAt(origin);

            PrimaryIds.InsertOrAppend(target, contentId);
            CoupledIds.InsertOrAppend(target, presentationId);
            return wrapLog("ok", BuildChangesList(PrimaryIds, CoupledIds));

        }



        public UpdateList Reorder(int[] newSequence)
        {
            var wrapLog = Log.Call<UpdateList>($"reorder all to:[{string.Join(",", newSequence)}]");

            // some error checks
            if (newSequence.Length != PrimaryIds.Count)
            {
                const string msg = "Error: Can't re-order - list length is different";
                wrapLog(msg, null);
                throw new Exception(msg);
            }

            var newPIds = new List<int?>();
            var newCIds = new List<int?>();
            var originalPIds = PrimaryIds.ToList();
            const int usedMarker = int.MinValue;

            foreach (var index in newSequence)
            {
                if(originalPIds[index] == usedMarker)
                    throw new Exception($"Cancelled re-order because index {index} was re-used");
                var primaryId = PrimaryIds[index];
                newPIds.Add(primaryId);
                originalPIds[index] = usedMarker;

                var coupledId = CoupledIds[index];
                newCIds.Add(coupledId);
                Log.Add($"Added at [{index}] values {primaryId}, {coupledId}");
            }

            PrimaryIds = newPIds;
            CoupledIds = newCIds;

            return wrapLog("ok", BuildChangesList(PrimaryIds, CoupledIds));
        }

        public UpdateList Replace(int index, int? entityId, bool updatePair, int? pairId)
        {
            var wrapLog = Log.Call<UpdateList>($"index: {index}, primaryId: {entityId}, coupled?:{updatePair}, coupledId:{pairId}");
            if (index == -1)
            {
                wrapLog("error", null);
                throw new Exception("Sort order is never -1 any more; deprecated");
            }

            // if necessary, increase length
            // this is important, because one list could be much longer already, then the other
            // must catch up
            PrimaryIds.EnsureListLength(index);

            var primaryChanged = ReplaceItemAtIndexIfChanged(PrimaryIds, index, entityId);
            var coupledChanged = false;

            // maybe do more
            if (updatePair)
            {
                Log.Add("Will update the coupled list");
                CoupledIds.EnsureListLength(index);
                coupledChanged = ReplaceItemAtIndexIfChanged(CoupledIds, index, pairId);
            }
            else
            {
                // otherwise ensure the lengths still match
                SyncCoupledListLength(PrimaryIds, CoupledIds);
            }
            var package = BuildChangesList(PrimaryIds, CoupledIds);

            return primaryChanged || coupledChanged ? package : null;
        }

        private static bool ReplaceItemAtIndexIfChanged(List<int?> listMain, int index, int? entityId)
        {
            if (listMain[index] == entityId) return false;
            listMain[index] = entityId;
            return true;
        }

        #region Basic Helpers

        /// <summary>
        /// This method does various clean-up, because historic data can often be mismatched.
        /// For example, a list-pair of content/presentation may not be in correct state, so this ensures that everything
        /// is in sync.
        /// </summary>
        /// <param name="primary"></param>
        /// <param name="coupled"></param>
        /// <returns></returns>
        private List<int?> SyncCoupledListLength(List<int?> primary, List<int?> coupled)
        {
            var wrapLog = Log.Call<List<int?>>();
            var difference = primary.Count - coupled.Count;
            Log.Add($"Difference: {difference}");

            // this should fix https://github.com/2sic/2sxc/issues/1178 and https://github.com/2sic/2sxc/issues/1158
            // goal is to simply remove the last presentation items, if there is a mismatch. Usually they will simply be nulls anyhow
            if (difference < 0)
            {
                Log.Add($"will remove {difference}");
                coupled.RemoveRange(primary.Count, -difference);
            }

            // extend as necessary
            if (difference > 0)
            {
                Log.Add($"will add {difference}");
                coupled.AddRange(Enumerable.Repeat(new int?(), difference));
            }

            return wrapLog("ok", coupled);
        }

        #endregion

        #region MiniHelpers

        private Dictionary<string, List<int?>> BuildChangesList(List<int?> primary, List<int?> coupled)
        {
            var wrapLog = Log.Call($"primary len: {primary.Count}; coupled len: {coupled?.Count}");
            var result = new Dictionary<string, List<int?>>(
                StringComparer.InvariantCultureIgnoreCase) {{PrimaryField, primary.ToList()}};
            if(coupled != null)
                result.Add(CoupledField, coupled);
            wrapLog("ok");
            return result;
        }

        #endregion
    }
}
