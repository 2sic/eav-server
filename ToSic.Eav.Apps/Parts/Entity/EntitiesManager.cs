using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Plumbing;
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

        private Import DbImporter => _import ?? (_import = _importLazy.Value.Init(Parent.ZoneId, Parent.AppId, false, false, Log));
        private Import _import;
        public EntitiesManager(
            Lazy<ImportListXml> lazyImportListXml, 
            Lazy<Import> importLazy, 
            Lazy<IImportExportEnvironment> environmentLazy, 
            SystemManager systemManager,
            IServiceProvider serviceProvider,
            LazyInitLog<EntitySaver> entitySaverLazy,
            AppsCacheSwitch appsCache, // Note: Singleton
            LazyInit<JsonSerializer> jsonSerializer,
            Generator<ExportListXml> exportListXmlGenerator
            ) : base("App.EntMan")
        {
            _lazyImportListXml = lazyImportListXml;
            _importLazy = importLazy;
            _environmentLazy = environmentLazy;
            _serviceProvider = serviceProvider;
            _entitySaverLazy = entitySaverLazy.SetLog(Log);
            _appsCache = appsCache;
            _exportListXmGenerator = exportListXmlGenerator;
            SystemManager = systemManager.Init(Log);
            Serializer = jsonSerializer.SetInit(j => j.Init(Parent.AppState, Log));
        }
        private readonly Lazy<ImportListXml> _lazyImportListXml;
        private readonly Lazy<Import> _importLazy;
        private readonly Lazy<IImportExportEnvironment> _environmentLazy;
        private IImportExportEnvironment Environment => _environment ?? (_environment = _environmentLazy.Value.Init(Log));
        private IImportExportEnvironment _environment;
        private readonly IServiceProvider _serviceProvider;
        private readonly LazyInitLog<EntitySaver> _entitySaverLazy;
        private readonly AppsCacheSwitch _appsCache;
        private readonly Generator<ExportListXml> _exportListXmGenerator;
        protected readonly SystemManager SystemManager;
        private LazyInit<JsonSerializer> Serializer { get; }

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
            var wrapLog = Log.Fn<List<int>>("", message: "save count:" + entities.Count + ", with Options:" + (saveOptions != null));

            // Run the change in a lock/transaction
            // This is to avoid parallel creation of new entities
            // because sometimes the save may be executed twice before the state knows that the entity exists
            // in which case it would add it twice
            var appState = Parent.AppState;

            saveOptions = saveOptions ?? Environment.SaveOptions(Parent.ZoneId); // SaveOptions.Build(Parent.ZoneId);

            // Inner call which will be executed with the Lock of the AppState
            List<int> InnerSaveInLock()
            {
                // ensure the type-definitions are real, not just placeholders
                foreach (var entity in entities)
                    if (entity is Entity e2
                        && !e2.Type.IsDynamic // it's not dynamic
                        && e2.Type.Attributes == null) // it doesn't have attributes, so it must have been in-memory
                    {
                        var newType = Parent.Read.ContentTypes.Get(entity.Type.Name);
                        if (newType != null) e2.UpdateType(newType); // try to update, but leave if not found
                    }
                
                // Clear Ephemeral attributes which shouldn't be saved (new in v12)
                entities.ForEach(e => ClearEphemeralAttributes(e));

                // attach relationship resolver - important when saving data which doesn't yet have the guid
                entities.ForEach(appState.Relationships.AttachRelationshipResolver);

                List<int> intIds = null;
                var dc = Parent.DataController;
                dc.DoButSkipAppCachePurge(() => intIds = dc.Save(entities, saveOptions));

                // Tell the cache to do a partial update
                _appsCache.Value.Update(_serviceProvider, Parent, intIds, Log);
                return intIds;
            }


            List<int> ids = null;
            appState.DoInLock(Log, () => ids = InnerSaveInLock());

            return wrapLog.Return(ids, $"ids:{ids.Count}");
        }


        /// <summary>
        /// WIP - clear attributes which shouldn't be saved at all
        /// </summary>
        /// <param name="entity"></param>
        private bool ClearEphemeralAttributes(IEntity entity)
        {
            var wrapLog = Log.Fn<bool>();
            var attributes = entity.Type?.Attributes;
            if(attributes == null || !attributes.Any()) return wrapLog.ReturnFalse("no attributes");

            var toClear = attributes.Where(a =>
                a.Metadata.GetBestValue<bool>(AttributeMetadata.MetadataFieldAllIsEphemeral) == true)
                .ToList();

            if (!toClear.Any()) return wrapLog.ReturnFalse("no ephemeral attributes");
            
            foreach (var a in toClear)
                if (entity.Attributes.TryGetValue(a.Name, out var attr))
                {
                    attr.Values.Clear();
                    Log.A("Cleared " + a.Name);
                }

            return wrapLog.ReturnTrue("cleared");
        }
    }
}
