using System.Collections.Immutable;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Build;

partial class DataFactory
{

    #region Create IRawEntity and Lists thereof

    /// <inheritdoc/>
    public IEntity Create(IRawEntitySource item)
    {
        // Get the raw entity using the extension which checks if it uses a converter or not.
        var raw = item.GetRawFromConverterOrDirectCast(MyOptions.RawConvertOptions);
        return CreateInternal(raw, typeGiver: item);
    }

    /// <inheritdoc />
    public IImmutableList<IEntity> Create<T>(IEnumerable<T> list) where T : class, IRawEntitySource
        => WrapUp(Prepare(list));

    #endregion


    #region Create single (internal)

    private IEntity CreateInternal(IRawEntity rawEntity, object typeGiver)
    {
        // ReSharper disable once RedundantAlwaysMatchSubpattern
        var partsBuilder = MyOptions.WithMetadata && rawEntity is IHasMetadata { Metadata: not null } typed
            ? new EntityPartsLazy(null, (_, _) => typed.Metadata)
            : null;

        // Set this the first time it's used, in case it should override the fallback content-type
        PctHelper.TypeFallbackIfNotSet ??= typeGiver.GetType();

        return Create(
            rawEntity.Values,
            id: rawEntity.Id,
            guid: rawEntity.Guid,
            created: rawEntity.Created,
            modified: rawEntity.Modified,
            partsBuilder: partsBuilder
        );
    }

    #endregion


    #region Prepare Convertibles to Pairs with raw entities

    private IList<EntityPair<IRawEntity>> Prepare<TNewEntity>(IEnumerable<TNewEntity> list)
        where TNewEntity : class, IRawEntitySource
    {
        var l = Log.Fn<IList<EntityPair<IRawEntity>>>();

        var all = list
            .Select(toBeRaw =>
            {
                try
                {
                    var reallyRaw = toBeRaw.GetRawFromConverterOrDirectCast(MyOptions.RawConvertOptions);
                    var newEntity = CreateInternal(reallyRaw, toBeRaw);
                    return new EntityPair<IRawEntity>(newEntity, reallyRaw);
                }
                catch
                {
                    // Add null to filter out later and report the indexes
                    return null;
                }
            })
            .ToListOpt();

        // Keep only non-nulls
        var cleaned = all
            .OfType<EntityPair<IRawEntity>>()
            .ToListOpt();

        // Verify we don't have nulls (errors) - if ok, exit now
        if (all.Count == cleaned.Count)
            return l.Return(cleaned);

        // if we have any nulls/errors, report the indexes
        var nullIndexes = all
            .Select((pair, index) => pair == null ? index : -1)     // Get index of null-entries
            .Where(index => index != -1)                            // drop the -1s (non-null entries)
            .ToListOpt();

        var msg = $"Error preparing: {nullIndexes.Count} items failed, indexes: {string.Join(",", nullIndexes)}";
        return l.Return(cleaned, msg);
    }

    #endregion


    #region WrapUp

    /// <summary>
    /// Finalize the work of building something, using prepared materials.
    /// </summary>
    /// <param name="rawList"></param>
    /// <returns></returns>
    private IImmutableList<IEntity> WrapUp(IEnumerable<EntityPair<IRawEntity>> rawList)
    {
        var l = Log.Fn<IImmutableList<IEntity>>();

        // Pre-process relationship keys, so they are added to the lookup
        var entityPairs = rawList.ToListOpt();

        // If the relationships are a lazy lookup, add all relationships to it
        if (Relationships is LazyLookup<object, IEntity> lazyRelationships)
            RelsConvertHelper.AddRelationshipsToLookup(entityPairs, lazyRelationships);

        // Return entities as Immutable list
        var result = entityPairs
            .Select(set => set.Entity)
            .ToImmutableOpt();
        return l.Return(result);
    }

    #endregion

}