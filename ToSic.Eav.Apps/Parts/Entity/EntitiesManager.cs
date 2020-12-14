using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Persistence;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.Parts
{
    /// <inheritdoc />
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public partial class EntitiesManager: PartOf<AppManager, EntitiesManager>
    {
        #region Constructor / DI

        private readonly Lazy<ImportListXml> _lazyImportListXml;
        private readonly Lazy<Import> _importLazy;

        private Import DbImporter => _import ?? (_import = _importLazy.Value.Init(Parent.ZoneId, Parent.AppId, false, false, Log));
        private Import _import;
        public EntitiesManager(Lazy<ImportListXml> lazyImportListXml, Lazy<Import> importLazy) : base("App.EntMan")
        {
            _lazyImportListXml = lazyImportListXml;
            _importLazy = importLazy;
        }
        
        #endregion


        public void Import(List<IEntity> newEntities)
        {
            newEntities.ForEach(e =>
            {
                e.ResetEntityId();
                if (Parent.Read.Entities.Get(e.EntityGuid) != null)
                    throw new ArgumentException("Can't import this item - an item with the same guid already exists");
            });
            Save(newEntities);
        }

        public int Save(IEntity entity, SaveOptions saveOptions = null) 
            => Save(new List<IEntity> {entity}, saveOptions).FirstOrDefault();

        public List<int> Save(List<IEntity> entities, SaveOptions saveOptions = null)
        {
            var wrapLog = Log.Call("", message: "save count:" + entities.Count + ", with Options:" + (saveOptions != null));

            // Run the change in a lock/transaction
            // This is to avoid parallel creation of new entities
            // because sometimes the save may be executed twice before the state knows that the entity exists
            // in which case it would add it twice
            var appState = Parent.AppState;

            // Inner call which will be executed with the Lock of the AppState
            List<int> InnerSaveInLock()
            {
                saveOptions = saveOptions ?? SaveOptions.Build(Parent.ZoneId);
                // ensure the type-definitions are real, not just placeholders
                foreach (var entity in entities)
                    if (entity is Entity e2
                        && !e2.Type.IsDynamic // it's not dynamic
                        && e2.Type.Attributes == null) // it doesn't have attributes, so it must have been in-memory
                    {
                        var newType = Parent.Read.ContentTypes.Get(entity.Type.Name);
                        if (newType != null) e2.UpdateType(newType); // try to update, but leave if not found
                    }

                // attach relationship resolver - important when saving data which doesn't yet have the guid
                entities.ForEach(appState.Relationships.AttachRelationshipResolver);

                List<int> intIds = null;
                var dc = Parent.DataController;
                dc.DoButSkipAppCachePurge(() => intIds = dc.Save(entities, saveOptions));

                // Tell the cache to do a partial update
                State.Cache.Update(Parent, intIds, Log);
                return intIds;
            }


            List<int> ids = null;
            appState.DoInLock(Log, () => ids = InnerSaveInLock());

            wrapLog($"ids:{ids.Count}");
            return ids;
        }
        
    }
}
