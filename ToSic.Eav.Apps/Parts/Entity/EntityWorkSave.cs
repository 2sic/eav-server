using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps.AppSys;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Persistence;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Plumbing;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static System.StringComparer;
using IEntity = ToSic.Eav.Data.IEntity;


namespace ToSic.Eav.Apps.Parts
{
    public class EntityWorkSave: AppWorkBase<IAppWorkCtxWithDb>
    {
        private readonly AppWork _appWork;
        private readonly LazySvc<IAppLoaderTools> _appLoaderTools;
        private readonly AppsCacheSwitch _appsCache;
        private readonly LazySvc<IImportExportEnvironment> _environmentLazy;

        public EntityWorkSave(
            AppWork appWork,
            LazySvc<DataBuilder> multiBuilder,
            LazySvc<IAppLoaderTools> appLoaderTools,
            AppsCacheSwitch appsCache, // Note: Singleton
            LazySvc<IImportExportEnvironment> environmentLazy
        ) : base("Wrk.EntSav")
        {
            ConnectServices(
                _appWork = appWork,
                _multiBuilder = multiBuilder,
                _appLoaderTools = appLoaderTools,
                _appsCache = appsCache,
                _environmentLazy = environmentLazy
            );
        }

        private readonly LazySvc<DataBuilder> _multiBuilder;
        private DataBuilder Builder => _multiBuilder.Value;


        public void Import(List<IEntity> newEntities)
        {
            foreach (var e in newEntities.Where(e => _appWork.Entities.Get(AppWorkCtx, e.EntityGuid) != null))
                throw new ArgumentException($"Can't import this item - an item with the same guid {e.EntityGuid} already exists");

            newEntities = newEntities
                .Select(e => Builder.Entity.CreateFrom(e, id: 0, repositoryId: 0))
                .ToList();
            Save(newEntities);
        }


        public int Save(IEntity entity, SaveOptions saveOptions = null)
            => Save(new List<IEntity> { entity }, saveOptions).FirstOrDefault();


        public List<int> Save(List<IEntity> entities, SaveOptions saveOptions = null) 
        {
            var l = Log.Fn<List<int>>("save count:" + entities.Count + ", with Options:" + (saveOptions != null));
            // Run the change in a lock/transaction
            // This is to avoid parallel creation of new entities
            // because sometimes the save may be executed twice before the state knows that the entity exists
            // in which case it would add it twice
            var appState = AppWorkCtx.AppState;

            saveOptions = saveOptions ?? _environmentLazy.Value.SaveOptions(appState.ZoneId);

            // Inner call which will be executed with the Lock of the AppState
            List<int> InnerSaveInLock()
            {

                // Try to reset the content-type if not specified
                entities = entities.Select(entity =>
                {
                    // If not Entity, or isDynamic, or no attributes (in-memory) leaves as is
                    if (!(entity is Entity e2) || e2.Type.IsDynamic || e2.Type.Attributes != null)
                        return entity;
                    var newType = appState.GetContentType(entity.Type.Name);
                    if (newType == null) return entity;

                    return Builder.Entity.CreateFrom(entity, type: newType);
                }).ToList();

                // Clear Ephemeral attributes which shouldn't be saved (new in v12)
                entities = entities.Select(entity =>
                {
                    var attributes = AttributesWithEmptyEphemerals(entity);
                    return attributes == null ? entity : Builder.Entity.CreateFrom(entity, attributes: attributes);
                }).ToList();

                // attach relationship resolver - important when saving data which doesn't yet have the guid
                entities = AttachRelationshipResolver(entities, appState);

                List<int> intIds = null;
                var dc = AppWorkCtx.DataController;
                dc.DoButSkipAppCachePurge(() => intIds = dc.Save(entities, saveOptions));

                // Tell the cache to do a partial update
                _appsCache.Value.Update(appState, intIds, Log, _appLoaderTools.Value);
                return intIds;
            }


            List<int> ids = null;
            appState.DoInLock(Log, () => ids = InnerSaveInLock());

            return l.Return(ids, $"ids:{ids.Count}");
        }


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
                    .Where(set => set.TypedContents?.Identifiers?.Count > 0)
                    .ToList();
                if (!relationshipAttributes.Any())
                    return e;

                // Create new attributes with updated relationship
                var relationshipsUpdated = relationshipAttributes
                    .Select(a =>
                    {
                        var newLazyEntities = Builder.Value.Relationships(a.TypedContents, appState);
                        return Builder.Attribute.CreateFrom(a.Attribute, newLazyEntities);
                    })
                    .ToList();

                // Assemble the attributes (replace the relationships)
                var attributes = Builder.Attribute.Replace(e.Attributes, relationshipsUpdated);

                // return cloned entity
                return Builder.Entity.CreateFrom(e, attributes: Builder.Attribute.Create(attributes));
            }).ToList();
            return updated;
        }


        /// <summary>
        /// WIP - clear attributes which shouldn't be saved at all
        /// </summary>
        /// <param name="entity"></param>
        private IImmutableDictionary<string, IAttribute> AttributesWithEmptyEphemerals(IEntity entity) => Log.Func(l =>
        {
            var attributes = entity.Type?.Attributes?.ToList();
            if (attributes == null || !attributes.Any()) return (null, "no attributes");

            var toClear = attributes
                .Where(a => a.Metadata.GetBestValue<bool>(AttributeMetadata.MetadataFieldAllIsEphemeral))
                .ToList();

            if (!toClear.Any()) return (null, "no ephemeral attributes");

            var result = entity.Attributes.ToImmutableDictionary(pair => pair.Key,
                pair =>
                {
                    if (!toClear.Any(tc => tc.Name.EqualsInsensitive(pair.Key)))
                        return pair.Value;
                    var empty = Builder.Attribute.CreateFrom(pair.Value, new List<IValue>().ToImmutableList());
                    l.A("Cleared " + pair.Key);
                    return empty;
                }, InvariantCultureIgnoreCase);

            return (result, "temp");
        });

    }
}
