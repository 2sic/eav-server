﻿using System.Collections.Immutable;
using ToSic.Eav.Apps.AppReader.Sys;
using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Data.Sys.Relationships;
using ToSic.Eav.Data.Sys.Save;
using ToSic.Eav.ImportExport.Integration;
using ToSic.Sys.Utils;
using static System.StringComparer;



namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkEntitySave(
    LazySvc<DataBuilder> multiBuilder,
    AppsCacheSwitch appsCache,
    LazySvc<IImportExportEnvironment> environmentLazy
    )
    : WorkUnitBase<IAppWorkCtxWithDb>("Wrk.EntSav", connect: [multiBuilder, appsCache, environmentLazy])
{
    // Note: Singleton

    private DataBuilder Builder => multiBuilder.Value;


    public void Import(List<IEntity> newEntities)
    {
        var appStateList = AppWorkCtx.AppReader.List;
        foreach (var e in newEntities.Where(e => appStateList.One(e.EntityGuid) != null))
            throw new ArgumentException($"Can't import this item - an item with the same guid {e.EntityGuid} already exists");

        var saveOptions = SaveOptions();
        var savePairs = newEntities
            .Select(e => Builder.Entity.CreateFrom(e, id: 0, repositoryId: 0))
            .Select(e => new EntityPair<SaveOptions>(e, saveOptions))
            .ToListOpt();

        Save(savePairs);
    }

    public SaveOptions SaveOptions()
        => environmentLazy.Value.SaveOptions(AppWorkCtx.ZoneId);

    public int Save(IEntity entity, SaveOptions saveOptions)
        => Save([new(entity, saveOptions)]).FirstOrDefault();


    public ICollection<int> Save(ICollection<EntityPair<SaveOptions>> entities)
    {
        var l = Log.Fn<ICollection<int>>($"save count:{entities.Count}");

        // Run the change in a lock/transaction
        // This is to avoid parallel creation of new entities
        // because sometimes the save may be executed twice before the state knows that the entity exists
        // in which case it would add it twice

        var appReader = AppWorkCtx.AppReader;
        List<int> ids = null!;
        appReader.GetCache().DoInLock(Log, () => ids = InnerSaveInLock());
        return l.Return(ids, $"ids:{ids.Count}");

        // Inner call which will be executed with the Lock of the AppState
        List<int> InnerSaveInLock()
        {
            // Try to reset the content-type if not specified
            entities = entities
                .Select(pair =>
                {
                    // If not Entity, or isDynamic, or no attributes (in-memory) leaves as is
                    if (pair.Entity is not Entity e2 || e2.Type.IsDynamic || e2.Type.Attributes != null)
                        return pair;

                    // Check if the attached type exists, if not, leave, otherwise ensure the type is attached
                    var newType = appReader.TryGetContentType(e2.Type.Name);
                    return newType == null
                        ? pair
                        : pair with { Entity = Builder.Entity.CreateFrom(pair.Entity, type: newType) };
                })
                .ToListOpt();

            // Clear Ephemeral attributes which shouldn't be saved (new in v12)
            entities = entities
                .Select(pair =>
                {
                    var attributes = AttributesWithEmptyEphemerals(pair.Entity);
                    return attributes == null
                        ? pair
                        : pair with { Entity = Builder.Entity.CreateFrom(pair.Entity, attributes: attributes) };
                })
                .ToListOpt();

            // attach relationship resolver - important when saving data which doesn't yet have the guid
            var pairsToSave = entities
                .Select(IEntityPair<SaveOptions> (p) => p with { Entity = AttachRelationshipResolver(p.Entity, appReader.GetCache()) })
                .ToListOpt();
            //entities = AttachRelationshipResolver(entities, appReader.GetCache());

            List<int> intIds = null!;
            var dc = AppWorkCtx.DbStorage;
            dc.DoButSkipAppCachePurge(() => intIds = dc.Save(pairsToSave));

            // Tell the cache to do a partial update
            appsCache.Update(appReader.PureIdentity(), intIds);
            return intIds;
        }
    }


    [PrivateApi]
    private IEntity AttachRelationshipResolver(IEntity entity, IEntitiesSource appState)
    {
        // Check if we have any relationships to update
        var relationshipAttributes = entity.Attributes
            .Select(a => a.Value)
            .Where(a => a is IAttribute<IEnumerable<IEntity>>)
            .Cast<IAttribute<IEnumerable<IEntity>>>()
            .Select(a => new
            {
                Attribute = a,
                TypedContents = a.TypedContents as IRelatedEntitiesValue,
            })
            .Where(set => set.TypedContents?.Count > 0)
            .ToListOpt();

        // If none, exit early
        if (!relationshipAttributes.Any())
            return entity;

        // Create new attributes with updated relationship
        var relationshipsUpdated = relationshipAttributes
            .Select(a =>
            {
                if (a.TypedContents == null)
                    return null!;
                var newLazyEntities = Builder.Value.Relationships(a.TypedContents, appState);
                return Builder.Attribute.CreateFrom(a.Attribute, newLazyEntities);
            })
            .Where(a => a != null)
            .ToListOpt();

        // Assemble the attributes (replace the relationships)
        var attributes = Builder.Attribute.Replace(entity.Attributes, relationshipsUpdated);

        // return cloned entity
        return Builder.Entity.CreateFrom(entity, attributes: Builder.Attribute.Create(attributes));
    }


    /// <summary>
    /// WIP - clear attributes which shouldn't be saved at all
    /// </summary>
    /// <param name="entity"></param>
    private IImmutableDictionary<string, IAttribute>? AttributesWithEmptyEphemerals(IEntity entity)
    {
        var l = Log.Fn<IImmutableDictionary<string, IAttribute>>();
        var attributes = entity.Type?.Attributes?.ToListOpt();
        if (attributes == null || !attributes.Any())
            return l.ReturnNull("no attributes");

        var toClear = attributes
            .Where(a => a.Metadata.Get<bool>(AttributeMetadataConstants.MetadataFieldAllIsEphemeral))
            .ToListOpt();

        if (!toClear.Any())
            return l.ReturnNull("no ephemeral attributes");

        var result = entity.Attributes.ToImmutableDictionary(pair => pair.Key,
            pair =>
            {
                if (!toClear.Any(tc => tc.Name.EqualsInsensitive(pair.Key)))
                    return pair.Value;
                var empty = Builder.Attribute.CreateFrom(pair.Value, []);
                l.A("Cleared " + pair.Key);
                return empty;
            }, InvariantCultureIgnoreCase);

        return l.Return(result, "temp");
    }

}