using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Eav.Generics;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.ImportExport.Serialization;
using ToSic.Lib.Logging;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Plumbing;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using static System.StringComparer;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.Parts
{
    /// <inheritdoc />
    /// <summary>
    /// Manager for entities in an app
    /// </summary>
    public partial class EntitiesManager: PartOf<AppManager>
    {
        #region Constructor / DI

        public EntitiesManager(
            LazySvc<ImportListXml> lazyImportListXml,
            LazySvc<Import> importLazy,
            LazySvc<IImportExportEnvironment> environmentLazy, 
            SystemManager systemManager,
            LazySvc<IAppLoaderTools> appLoaderTools,
            LazySvc<EntitySaver> entitySaverLazy,
            AppsCacheSwitch appsCache, // Note: Singleton
            LazySvc<JsonSerializer> jsonSerializer,
            LazySvc<DataBuilder> multiBuilder,
            Generator<ExportListXml> exportListXmlGenerator
            ) : base("App.EntMan")
        {
            ConnectServices(
                _lazyImportListXml = lazyImportListXml,
                _importLazy = importLazy,
                _environmentLazy = environmentLazy,
                SystemManager = systemManager,
                _appLoaderTools = appLoaderTools,
                _entitySaverLazy = entitySaverLazy,
                _appsCache = appsCache,
                _exportListXmGenerator = exportListXmlGenerator,
                _multiBuilder = multiBuilder,
                Serializer = jsonSerializer.SetInit(j => j.SetApp(Parent.AppState))
            );
        }
        private readonly LazySvc<ImportListXml> _lazyImportListXml;
        private readonly LazySvc<Import> _importLazy;
        private readonly LazySvc<IImportExportEnvironment> _environmentLazy;
        private readonly LazySvc<IAppLoaderTools> _appLoaderTools;
        private readonly LazySvc<EntitySaver> _entitySaverLazy;
        private readonly AppsCacheSwitch _appsCache;
        private readonly Generator<ExportListXml> _exportListXmGenerator;
        protected readonly SystemManager SystemManager;
        private LazySvc<JsonSerializer> Serializer { get; }

        private Import DbImporter => _import ?? (_import = _importLazy.Value.Init(Parent.ZoneId, Parent.AppId, false, false));
        private Import _import;

        private readonly LazySvc<DataBuilder> _multiBuilder;
        private DataBuilder Builder => _multiBuilder.Value;

        #endregion


        public void Import(List<IEntity> newEntities)
        {
            foreach (var e in newEntities.Where(e => Parent.Read.Entities.Get(e.EntityGuid) != null))
                throw new ArgumentException($"Can't import this item - an item with the same guid {e.EntityGuid} already exists");

            newEntities = newEntities
                .Select(e => Builder.Entity.Clone(e, id: 0, repositoryId: 0))
                .ToList();
            Save(newEntities);
        }

        public int Save(IEntity entity, SaveOptions saveOptions = null) 
            => Save(new List<IEntity> {entity}, saveOptions).FirstOrDefault();

        public List<int> Save(List<IEntity> entities, SaveOptions saveOptions = null
        ) => Log.Func(message: "save count:" + entities.Count + ", with Options:" + (saveOptions != null), func: () =>
        {
            // Run the change in a lock/transaction
            // This is to avoid parallel creation of new entities
            // because sometimes the save may be executed twice before the state knows that the entity exists
            // in which case it would add it twice
            var appState = Parent.AppState;

            saveOptions = saveOptions ?? _environmentLazy.Value.SaveOptions(Parent.ZoneId);

            // Inner call which will be executed with the Lock of the AppState
            List<int> InnerSaveInLock()
            {

                // Try to reset the content-type if not specified
                entities = entities.Select(entity =>
                {
                    // If not Entity, or isDynamic, or no attributes (in-memory) leaves as is
                    if (!(entity is Entity e2) || e2.Type.IsDynamic || e2.Type.Attributes != null)
                        return entity;
                    var newType = Parent.Read.ContentTypes.Get(entity.Type.Name);
                    if (newType == null) return entity;

                    return Builder.Entity.Clone(entity, type: newType);
                }).ToList();

                // Clear Ephemeral attributes which shouldn't be saved (new in v12)
                entities = entities.Select(entity =>
                {
                    var attributes = AttributesWithEmptyEphemerals(entity);
                    return attributes == null ? entity : Builder.Entity.Clone(entity, attributes: attributes);
                }).ToList();

                // attach relationship resolver - important when saving data which doesn't yet have the guid
                entities = AttachRelationshipResolver(entities, appState);

                List<int> intIds = null;
                var dc = Parent.DataController;
                dc.DoButSkipAppCachePurge(() => intIds = dc.Save(entities, saveOptions));

                // Tell the cache to do a partial update
                _appsCache.Value.Update(Parent, intIds, Log, _appLoaderTools.Value);
                return intIds;
            }


            List<int> ids = null;
            appState.DoInLock(Log, () => ids = InnerSaveInLock());

            return (ids, $"ids:{ids.Count}");
        });

        [PrivateApi]
        public List<IEntity> AttachRelationshipResolver(List<IEntity> entities, AppState appState)
        {
            var updated = entities.Select(e =>
            {
                // Check if we have any relationships to update
                var relationshipAttributes = e.Attributes
                    .Select(a => a.Value)
                    .Where(a => a is IAttribute<IEnumerable<IEntity>>)
                    .Cast<IAttribute<IEnumerable<IEntity>>>()
                    .Select(a => new
                    {
                        Attribute = a,
                        TypedContents = a.TypedContents as IRelatedEntitiesValue,
                    })
                    .Where(set => set.TypedContents != null && set.TypedContents.Identifiers?.Any() == true)
                    .ToList();
                if (!relationshipAttributes.Any())
                    return e;

                // Create new attributes with updated relationship
                var relationshipsUpdated = relationshipAttributes
                    .Select(a =>
                    {
                        var newLazyEntities = Builder.Value.CloneRelationship(a.TypedContents, appState);
                        return Builder.Attribute.Clone(a.Attribute,
                            new List<IValue> { newLazyEntities }.ToImmutableList());
                    })
                    .ToList();

                // Assemble the attributes (replace the relationships)
                var attributes = e.Attributes.ToEditable();
                foreach (var updatedRel in relationshipsUpdated)
                    Builder.Attribute.Replace(attributes, updatedRel);

                // return cloned entity
                return Builder.Entity.Clone(e, attributes: Builder.Attribute.Create(attributes));
            }).ToList();
            return updated;
        }


        /// <summary>
        /// WIP - clear attributes which shouldn't be saved at all
        /// </summary>
        /// <param name="entity"></param>
        private IImmutableDictionary<string, IAttribute> AttributesWithEmptyEphemerals(IEntity entity) => Log.Func(() =>
        {
            var attributes = entity.Type?.Attributes;
            if (attributes == null || !attributes.Any()) return (null, "no attributes");

            var toClear = attributes.Where(a =>
                    a.Metadata.GetBestValue<bool>(AttributeMetadata.MetadataFieldAllIsEphemeral))
                .ToList();

            if (!toClear.Any()) return (null, "no ephemeral attributes");

            var result = entity.Attributes.ToImmutableDictionary(pair => pair.Key,
                pair =>
                {
                    if (!toClear.Any(tc => tc.Name.EqualsInsensitive(pair.Key)))
                        return pair.Value;
                    var empty = Builder.Attribute.Clone(pair.Value, new List<IValue>().ToImmutableList());
                    Log.A("Cleared " + pair.Key);
                    return empty;
                }, InvariantCultureIgnoreCase);

            return (result, "temp");
        });
    }
}
