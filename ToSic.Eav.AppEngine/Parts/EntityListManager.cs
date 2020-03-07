using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// This is responsible for managing / changing lists of entities.
    /// These are usually relationship-properties of an entity, like the Content items on a ContentBlock
    /// </summary>
    public class EntityListManager : ManagerBase
    {
        public IEntity Entity;
        public string PrimaryField;
        public string CoupledField;
        public bool VersioningEnabled;

        //internal List<int?> PrimaryIds;
        //internal List<int?> CoupledIds;

        internal ListPair ListPair;


        public EntityListManager(AppManager app, IEntity entity, string primaryField, string coupledField,
            bool versioningEnabled, ILog parentLog)
            : base(app, parentLog, "App.LstMng")
        {
            Entity = entity;
            PrimaryField = primaryField;
            CoupledField = coupledField;
            var primaryIds = IdsWithNulls(Entity.Children(PrimaryField));
            var coupledIds = IdsWithNulls(Entity.Children(CoupledField));
            //var coupledIds = SyncCoupledListLength(primaryIds, initialCoupledIds);
            VersioningEnabled = versioningEnabled;

            ListPair = new ListPair(primaryIds, coupledIds, primaryField, coupledField, Log);
        }

        /// <summary>
        /// If SortOrder is not specified, adds at the end
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <param name="primaryId"></param>
        /// <param name="coupledId"></param>
        public void Add(int? sortOrder, int? primaryId, int? coupledId)
        {
            //Log.Add($"add content/pres at order:{sortOrder}, content#{primaryId}, pres#{coupledId}");

            //if (!sortOrder.HasValue)
            //    sortOrder = PrimaryIds.Count;

            //PrimaryIds.InsertOrAppend(sortOrder.Value, primaryId);
            //CoupledIds.InsertOrAppend(sortOrder.Value, coupledId);

            //var values = BuildChangesList(PrimaryIds, CoupledIds);
            var values = ListPair.Add(sortOrder, primaryId, coupledId);
            Save(values);
        }

        /// <summary>
        /// Removes entities from a group. This will also remove the corresponding presentation entities.
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            //Log.Add($"remove content and pres items type:{PrimaryField}, order:{index}");
            //PrimaryIds.RemoveAt(index);
            //// in many cases the presentation-list is empty, then there is nothing to remove
            //if (CoupledIds.Count > index)
            //    CoupledIds.RemoveAt(index);
            //var values = BuildChangesList(PrimaryIds, CoupledIds);
            var values = ListPair.Remove(index);
            Save(values);
        }


        public void Move(int origin, int target)
        {
            //var wrapLog = Log.Call($"reorder entities before:{origin} to after:{target}");
            ////CoupledIds = SyncCoupledListLength(PrimaryIds, CoupledIds);
            //var contentId = PrimaryIds[origin];
            //var presentationId = CoupledIds[origin];

            ///*
            // * ToDo 2017-08-28:
            // * Create a DRAFT copy of the BlockConfiguration if versioning is enabled.
            // */

            //PrimaryIds.RemoveAt(origin);
            //CoupledIds.RemoveAt(origin);

            //PrimaryIds.InsertOrAppend(target, contentId);
            //CoupledIds.InsertOrAppend(target, presentationId);

            //var list = BuildChangesList(PrimaryIds, CoupledIds);
            var list = ListPair.Move(origin, target);
            Save(list);
            //wrapLog("ok");
        }




        private void Save(Dictionary<string, List<int?>> values)
        {
            var wrapLog = Log.Call();
            if (values == null || !values.Any())
            {
                wrapLog("nothing to save");
                return;
            }
            // ensure that all sub-lists have the same count-number
            // but ignore empty lists, because as of now, we often have non-pairs
            // when presentation can be empty
            var countGroups = values
                .GroupBy(kvp => kvp.Value.Count)
                .Where(grp => grp.Count() != 0);
            if (countGroups.Count() != 1)
            {
                const string msg = "Error: Paired Lists must be same length, they are not.";
                wrapLog(msg);
                throw new Exception(msg);
            }

            var dicObj = values.ToDictionary(x => x.Key, x => x.Value as object);
            var newEnt = new Entity(AppManager.AppId, 0, Entity.Type, dicObj);
            var saveOpts = SaveOptions.Build(AppManager.ZoneId);
            saveOpts.PreserveUntouchedAttributes = true;

            var saveEnt = new EntitySaver(Log)
                .CreateMergedForSaving(Entity, newEnt, saveOpts);

            Log.Add($"VersioningEnabled:{VersioningEnabled}");
            if (VersioningEnabled)
            {
                // Force saving as draft if needed (if versioning is enabled)
                ((Entity)saveEnt).PlaceDraftInBranch = true;
                ((Entity)saveEnt).IsPublished = false;
            }

            AppManager.Entities.Save(saveEnt, saveOpts);
            wrapLog("ok");
        }

        public void Reorder(int[] newSequence)
        {
            //var wrapLog = Log.Call(() => $"reorder all to:[{string.Join(",", newSequence)}]");
            ////CoupledIds = SyncCoupledListLength(PrimaryIds, CoupledIds);

            //// some error checks
            //if (newSequence.Length != PrimaryIds.Count)
            //{
            //    const string msg = "Error: Can't re-order - list length is different";
            //    wrapLog(msg);
            //    throw new Exception(msg);
            //}

            //var newContentIds = new List<int?>();
            //var newPresIds = new List<int?>();

            //foreach (var index in newSequence)
            //{
            //    var primaryId = PrimaryIds[index];
            //    newContentIds.Add(primaryId);

            //    var coupledId = CoupledIds[index];
            //    newPresIds.Add(coupledId);
            //    Log.Add($"Added at [{index}] values {primaryId}, {coupledId}");
            //}

            //var list = BuildChangesList(newContentIds, newPresIds);
            var list = ListPair.Reorder(newSequence);
            Save(list);
            //wrapLog("ok");
            //return true;
        }

        public void Replace(int index, int? entityId, bool updatePair, int? pairId)
        {
            //var wrapLog = Log.Call($"index: {index}, primaryId: {entityId}, coupled?:{updatePair}, coupledId:{pairId}");
            //if (index == -1)
            //{
            //    wrapLog("error");
            //    throw new Exception("Sort order is never -1 any more; deprecated");
            //}

            //// if necessary, increase length
            //PrimaryIds.EnsureListLength(index);

            //var somethingChanged = ReplaceItemAtIndexIfChanged(PrimaryIds, index, entityId);

            //// maybe do more
            //if (updatePair)
            //{
            //    Log.Add("Will update the coupled list");
            //    CoupledIds.EnsureListLength(index);
            //    somethingChanged = somethingChanged || ReplaceItemAtIndexIfChanged(CoupledIds, index, pairId);
            //}
            //var package = BuildChangesList(PrimaryIds, CoupledIds);
            var package = ListPair.Replace(index, entityId, updatePair, pairId);
            //if (package != null)
                Save(package);
        }

        //private static bool ReplaceItemAtIndexIfChanged(List<int?> listMain, int index, int? entityId)
        //{
        //    if (listMain[index] == entityId) return false;
        //    listMain[index] = entityId;
        //    return true;
        //}

        #region Basic Helpers

        ///// <summary>
        ///// This method does various clean-up, because historic data can often be mismatched.
        ///// For example, a list-pair of content/presentation may not be in correct state, so this ensures that everything
        ///// is in sync.
        ///// </summary>
        ///// <param name="primary"></param>
        ///// <param name="coupled"></param>
        ///// <returns></returns>
        //private List<int?> SyncCoupledListLength(List<int?> primary, List<int?> coupled)
        //{
        //    var wrapLog = Log.Call<List<int?>>();
        //    var difference = primary.Count - coupled.Count;
        //    Log.Add($"Difference: {difference}");

        //    // this should fix https://github.com/2sic/2sxc/issues/1178 and https://github.com/2sic/2sxc/issues/1158
        //    // goal is to simply remove the last presentation items, if there is a mismatch. Usually they will simply be nulls anyhow
        //    if (difference < 0)
        //    {
        //        Log.Add($"will remove {difference}");
        //        coupled.RemoveRange(primary.Count, difference);
        //    }

        //    // extend as necessary
        //    if (difference > 0)
        //    {
        //        Log.Add($"will add {difference}");
        //        coupled.AddRange(Enumerable.Repeat(new int?(), difference));
        //    }

        //    return wrapLog("ok", coupled);
        //}

        #endregion

        #region MiniHelpers

        //private Dictionary<string, List<int?>> BuildChangesList(List<int?> primary, List<int?> coupled)
        //{
        //    var wrapLog = Log.Call($"primary len: {primary.Count}; coupled len: {coupled?.Count}");
        //    var result = new Dictionary<string, List<int?>>(
        //        StringComparer.InvariantCultureIgnoreCase) {{PrimaryField, primary.ToList()}};
        //    if(coupled != null)
        //        result.Add(CoupledField, coupled);
        //    wrapLog("ok");
        //    return result;
        //}


        private static List<int?> IdsWithNulls(IEnumerable<IEntity> list)
            => list.Select(p => p?.EntityId).ToList();

        #endregion
    }
}
