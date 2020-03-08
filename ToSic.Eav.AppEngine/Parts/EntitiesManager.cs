using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.Parts
{
    /// <inheritdoc />
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public partial class EntitiesManager: ManagerBase
    {
        public EntitiesManager(AppManager app, ILog parentLog = null) 
            : base(app, parentLog, "App.EntMan")
        {
        }


        public int Save(IEntity entity, SaveOptions saveOptions = null) 
            => Save(new List<IEntity> {entity}, saveOptions).FirstOrDefault();

        public List<int> Save(List<IEntity> entities, SaveOptions saveOptions = null)
        {
            var wrapLog = Log.Call("", message: "save count:" + entities.Count + ", with Options:" + (saveOptions != null));
            saveOptions = saveOptions ?? SaveOptions.Build(AppManager.ZoneId);

            // ensure the type-definitions are real, not just placeholders
            foreach (var entity in entities)
                if (entity is Entity e2
                    && !e2.Type.IsDynamic // it's not dynamic
                    && e2.Type.Attributes == null) // it doesn't have attributes, so it must have been in-memory
                {
                    var newType = AppManager.Read.ContentTypes.Get(entity.Type.Name);
                    if(newType != null) e2.UpdateType(newType); // try to update, but leave if not found
                }

            // attach relationship resolver - important when saving data which doesn't yet have the guid
            entities.ForEach(AppManager.AppState.Relationships.AttachRelationshipResolver);

            List<int> ids = null;
            AppManager.DataController.DoButSkipAppCachePurge(() =>
                AppManager.DataController.DoWhileQueueingRelationships(() =>
                {
                    ids = AppManager.DataController.Save(entities, saveOptions);
                })
            );

            // Tell the cache to do a partial update
            State.Cache.Update(AppManager, ids, Log);

            wrapLog($"ids:{ids.Count}");
            return ids;
        }






        #region Helpers to get things done
        // todo: probably should move to the new Eav.Apps section, but for that we must
        public void ModifyItemList(int parentId, string field, IItemListAction actionToPerform)
        {
            Log.Add($"modify item list parent:{parentId}, field:{field}, action:{actionToPerform}");
            var parentEntity = AppManager.Read.Entities.Get(parentId);
            var parentField = parentEntity.GetBestValue(field);

            if (!(parentField is IEnumerable<IEntity> fieldList))
                throw new Exception("field " + field + " doesn't seem to be a list of content-items, must abort");

            var ids = actionToPerform.Change(fieldList.ToList());
            if (ids == null) return;

            // save
            var values = new Dictionary<string, object> { { field, ids.Select(e => e?.EntityGuid).ToList() } };
            AppManager.Entities.UpdateParts(parentEntity.EntityId, values);
        }
        #endregion


    }
}
