using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence;
using UpdateList = System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int?>>;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// This is responsible for managing / changing lists of entities.
    /// These are usually relationship-properties of an entity, like the Content items on a ContentBlock
    /// </summary>
    public class EntityListEditor : ManagerBase
    {
        public IEntity Entity;
        public bool VersioningEnabled;

        internal ListPair ListPair;


        public EntityListEditor(AppManager app, IEntity entity, string primaryField, string coupledField,
            bool versioningEnabled, ILog parentLog)
            : base(app, parentLog, "App.LstMng")
        {
            Entity = entity;
            var primaryIds = IdsWithNulls(entity.Children(primaryField));
            var coupledIds = IdsWithNulls(entity.Children(coupledField));
            VersioningEnabled = versioningEnabled;

            ListPair = new ListPair(primaryIds, coupledIds, primaryField, coupledField, Log);
        }

        //public ListPair GetMultiList(IEntity entity, string primaryField, string coupledField)
        //{
        //    var primaryIds = IdsWithNulls(entity.Children(primaryField));
        //    var coupledIds = IdsWithNulls(entity.Children(coupledField));
        //    return new ListPair(primaryIds, coupledIds, primaryField, coupledField, Log);
        //}

        /// <summary>
        /// If SortOrder is not specified, adds at the end
        /// </summary>
        /// <param name="sortOrder"></param>
        /// <param name="primaryId"></param>
        /// <param name="coupledId"></param>
        public void Add(int? sortOrder, int? primaryId, int? coupledId) 
            => Save(Entity, ListPair.Add(sortOrder, new[]{ primaryId, coupledId }));

        /// <summary>
        /// Removes entities from a group. This will also remove the corresponding presentation entities.
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index) 
            => Save(Entity, ListPair.Remove(index));


        public void Move(int origin, int target)
        {
            /*
             * Known Issue 2017-08-28:
             * Should case DRAFT copy of the BlockConfiguration if versioning is enabled.
             */
            Save(Entity, ListPair.Move(origin, target));
        }


        public void Save(Func<UpdateList> innerMethod) => Save(Entity, innerMethod.Invoke());

        private void Save(IEntity entity, UpdateList values)
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
            var newEnt = new Entity(AppManager.AppId, 0, entity.Type, dicObj);
            var saveOpts = SaveOptions.Build(AppManager.ZoneId);
            saveOpts.PreserveUntouchedAttributes = true;

            var saveEnt = new EntitySaver(Log)
                .CreateMergedForSaving(entity, newEnt, saveOpts);

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
            => Save(Entity, ListPair.Reorder(newSequence));

        public void Replace(int index, Tuple<bool, int?>[] values) 
            => Save(Entity, ListPair.Replace(index, values));

        #region MiniHelpers

        private static List<int?> IdsWithNulls(IEnumerable<IEntity> list)
            => list.Select(p => p?.EntityId).ToList();

        #endregion
    }
}
