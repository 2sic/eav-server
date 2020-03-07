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
            PrimaryIds.RemoveAt(index);
            // in many cases the presentation-list is empty, then there is nothing to remove
            if (CoupledIds.Count > index)
                CoupledIds.RemoveAt(index);
            return BuildChangesList(PrimaryIds, CoupledIds);
        }


        public UpdateList Move(int origin, int target)
        {
            var wrapLog = Log.Call<UpdateList>($"reorder entities before:{origin} to after:{target}");
            var contentId = PrimaryIds[origin];
            var presentationId = CoupledIds[origin];

            /*
             * ToDo 2017-08-28:
             * Create a DRAFT copy of the BlockConfiguration if versioning is enabled.
             */

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

            var newContentIds = new List<int?>();
            var newPresIds = new List<int?>();

            foreach (var index in newSequence)
            {
                var primaryId = PrimaryIds[index];
                newContentIds.Add(primaryId);

                var coupledId = CoupledIds[index];
                newPresIds.Add(coupledId);
                Log.Add($"Added at [{index}] values {primaryId}, {coupledId}");
            }

            return wrapLog("ok", BuildChangesList(newContentIds, newPresIds));
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
            PrimaryIds.EnsureListLength(index);

            var somethingChanged = ReplaceItemAtIndexIfChanged(PrimaryIds, index, entityId);

            // maybe do more
            if (updatePair)
            {
                Log.Add("Will update the coupled list");
                CoupledIds.EnsureListLength(index);
                somethingChanged = somethingChanged || ReplaceItemAtIndexIfChanged(CoupledIds, index, pairId);
            }
            var package = BuildChangesList(PrimaryIds, CoupledIds);

            return somethingChanged ? package : null;
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


        //private static List<int?> IdsWithNulls(IEnumerable<IEntity> list)
        //    => list.Select(p => p?.EntityId).ToList();

        #endregion
    }
}
