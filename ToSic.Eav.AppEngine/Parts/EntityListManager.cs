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
            => Save(ListPair.Add(sortOrder, primaryId, coupledId));

        /// <summary>
        /// Removes entities from a group. This will also remove the corresponding presentation entities.
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index) 
            => Save(ListPair.Remove(index));


        public void Move(int origin, int target)
        {
            /*
             * Known Issue 2017-08-28:
             * Create a DRAFT copy of the BlockConfiguration if versioning is enabled.
             */
            Save(ListPair.Move(origin, target));
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
            var package = ListPair.Replace(index, entityId, updatePair, pairId);
            Save(package);
        }

        #region MiniHelpers

        private static List<int?> IdsWithNulls(IEnumerable<IEntity> list)
            => list.Select(p => p?.EntityId).ToList();

        #endregion
    }
}
