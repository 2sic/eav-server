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

        public List<int?> GetPrimaryIds() => IdsWithNulls(Entity.Children(PrimaryField));
        public List<int?> GetCoupledIds() => IdsWithNulls(Entity.Children(CoupledField));


        public EntityListManager(
            AppManager app,
            IEntity entity,
            string primaryField,
            string coupledField,
            bool versioningEnabled,
            ILog parentLog)
            : base(app, parentLog, "App.LstMng")
        {
            Entity = entity;
            PrimaryField = primaryField;
            CoupledField = coupledField;
            VersioningEnabled = versioningEnabled;
        }

        /// <summary>
        /// If SortOrder is not specified, adds at the end
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <param name="contentId"></param>
        /// <param name="presentationId"></param>
        public void AddContentAndPresentationEntity(int? sortOrder, int? contentId, int? presentationId)
        {
            Log.Add($"add content/pres at order:{sortOrder}, content#{contentId}, pres#{presentationId}");
            //var type = ViewParts.ContentLower;
            //if (type.ToLower() != ViewParts.ContentLower)
            //    throw new Exception("This is only meant to work for content, not for list-content");

            //if (!sortOrder.HasValue)
            //    sortOrder = block.Content.Count;

            // get lists and ensure second list is already the same length before changes
            var list1 = GetPrimaryIds(); // IdsWithNulls(block.Content);// block.ListWithNulls(type);
            var list2 = SyncPairedListLengths(list1, GetCoupledIds());

            if (!sortOrder.HasValue)
                sortOrder = list1.Count;

            InsertOrAppend(list1, sortOrder.Value, contentId);
            InsertOrAppend(list2,sortOrder.Value, presentationId);
            //list1.Insert(sortOrder.Value, contentId);
            //list2.Insert(sortOrder.Value, presentationId);
            // post-pad just to be safe
            list2 = SyncPairedListLengths(list1, list2);

            var values = BuildChangesList(list1, list2);
            SavePairedLists(values);
        }

        /// <summary>
        /// Removes entities from a group. This will also remove the corresponding presentation entities.
        /// </summary>
        /// <param name="sortOrder"></param>
        public void RemoveInPair(int sortOrder)
        {
            Log.Add($"remove content and pres items type:{PrimaryField}, order:{sortOrder}");
            //var list1 = block.ListWithNulls(type);
            var list1 = GetPrimaryIds(); // IdsWithNulls(Entity.Children(fieldPrimary));
            list1.RemoveAt(sortOrder);
            //var type2 = ReCapitalizePartName(type).Replace(ViewParts.Content, ViewParts.Presentation);
            //var list2 = block.ListWithNulls(type2);
            var list2 = GetCoupledIds(); // IdsWithNulls(entity.Children(fieldCoupled));
            if (list2.Count > sortOrder
            ) // in many cases the presentation-list is empty, then there is nothing to remove
                list2.RemoveAt(sortOrder);
            var values = BuildChangesList(list1, list2);
                //ConcatChanges(CoupledField, list2));
            SavePairedLists( /*app,*/ /*entity, versioningEnabled,*/ values);
            //SaveItemLists(block, values);
        }


        public void ReorderAndSave(int sortOrder, int destinationSortOrder)
        {
            var wrapLog = Log.Call($"reorder entities before:{sortOrder} to after:{destinationSortOrder}");
            //var primary = Entity.Children(fieldPrimary);
            //var coupled = Entity.Children(fieldCoupled);
            var contentIds = GetPrimaryIds(); // IdsWithNulls(primary);// block.ListWithNulls(ViewParts.Content);
            var presentationIds = SyncPairedListLengths(contentIds, GetCoupledIds());
            // GetPresentationIdWithSameLengthAsContent(block);
            var contentId = contentIds[sortOrder];
            var presentationId = presentationIds[sortOrder];

            /*
             * ToDo 2017-08-28:
             * Create a DRAFT copy of the BlockConfiguration if versioning is enabled.
             */

            contentIds.RemoveAt(sortOrder);
            presentationIds.RemoveAt(sortOrder);

            InsertOrAppend(contentIds, destinationSortOrder, contentId);
            InsertOrAppend(presentationIds, destinationSortOrder, presentationId);
            //contentIds.Insert(destinationSortOrder, contentId);
            //presentationIds.Insert(destinationSortOrder, presentationId);

            var list = BuildChangesList(contentIds, presentationIds);
            SavePairedLists(list);
            wrapLog("ok");
        }

        private static void InsertOrAppend(IList<int?> list, int position, int? value)
        {
            if(list.Count > position)
                list.Insert(position, value);
            else
                list.Add(value);
        }



        public void SavePairedLists(Dictionary<string, List<int?>> values)
        {
            var wrapLog = Log.Call();
            if (!values.Any())
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

            //if (values.ContainsKey(ViewParts.Presentation))
            //{
            //    var contentCount = block.Content.Count;
            //    if (values.ContainsKey(ViewParts.Content))
            //        contentCount = values[ViewParts.Content].Count;
            //    if (values[ViewParts.Presentation].Count > contentCount)
            //        throw new Exception("Presentation may not contain more items than Content.");
            //}

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

        public bool ReorderAllAndSave(int[] newSequence)
        {
            var wrapLog = Log.Call(() => $"reorder all to:[{string.Join(",", newSequence)}]");
            var oldCIds = GetPrimaryIds();
            var oldPIds = SyncPairedListLengths(oldCIds, GetCoupledIds());

            // some error checks
            if (newSequence.Length != oldCIds.Count)
            {
                const string msg = "Error: Can't re-order - list length is different";
                wrapLog(msg);
                throw new Exception(msg);
            }

            var newContentIds = new List<int?>();
            var newPresIds = new List<int?>();

            foreach (var index in newSequence)
            {
                var primaryId = oldCIds[index];
                newContentIds.Add(primaryId);

                var coupledId = oldPIds[index];
                newPresIds.Add(coupledId);
                Log.Add($"Added at [{index}] values {primaryId}, {coupledId}");
            }

            var list = BuildChangesList(newContentIds, newPresIds);
            SavePairedLists(list);
            wrapLog("ok");
            return true;
        }

        public void UpdateEntityIfChanged(int index, int? entityId, bool updatePair, int? pairId)
        {
            var wrapLog = Log.Call($"index: {index}, primaryId: {entityId}, coupled?:{updatePair}, coupledId:{pairId}");
            var somethingChanged = false;
            if (index == -1)
            {
                wrapLog("error");
                throw new Exception("Sort order is never -1 any more; deprecated");
            }

            var primary = GetPrimaryIds();
            var coupled = GetCoupledIds();

            // if necessary, increase length
            EnsureIdListLength(primary, index);

            somethingChanged = ReplaceItemAtIndexIfChanged(primary, index, entityId);

            // maybe do more
            if (updatePair)
            {
                Log.Add("Will update the coupled list");
                EnsureIdListLength(coupled, index);
                somethingChanged = somethingChanged || ReplaceItemAtIndexIfChanged(coupled, index, pairId);
            }
            var package = BuildChangesList(primary, coupled);

            if (somethingChanged)
                SavePairedLists(package);
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
        private List<int?> SyncPairedListLengths(List<int?> primary, List<int?> coupled)
        {
            var wrapLog = Log.Call<List<int?>>();
            var difference = primary.Count - coupled.Count;
            Log.Add($"Difference: {difference}");

            // this should fix https://github.com/2sic/2sxc/issues/1178 and https://github.com/2sic/2sxc/issues/1158
            // goal is to simply remove the last presentation items, if there is a mismatch. Usually they will simply be nulls anyhow
            if (difference < 0)
            {
                Log.Add($"will remove {difference}");
                coupled.RemoveRange(primary.Count, difference);
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

        private static void EnsureIdListLength(List<int?> listMain, int sortOrder)
        {
            if (listMain.Count < sortOrder + 1)
                listMain.AddRange(Enumerable.Repeat(new int?(), (sortOrder + 1) - listMain.Count));
        }


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


        private static List<int?> IdsWithNulls(IEnumerable<IEntity> list)
            => list.Select(p => p?.EntityId).ToList();

        #endregion
    }
}
