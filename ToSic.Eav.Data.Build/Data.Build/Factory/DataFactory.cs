using System.Collections.Immutable;
using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.Entities.Sources;
using ToSic.Eav.Data.Sys.EntityPair;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Data.Build;

[PrivateApi("hide implementation")]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal class DataFactory(
    Generator<DataAssembler, DataAssemblerOptions> dataAssembler,
    LazySvc<ContentTypeAssembler> typeAssembler,
    Generator<IDataFactory, DataFactoryOptions> selfGenerator,
    LazySvc<ContentTypesFromCodeManager> codeCtManager)
    : ServiceWithSetup<DataFactoryOptions>("Ds.DatBld", connect: [dataAssembler, typeAssembler, selfGenerator, codeCtManager]), IDataFactory
{

    #region Properties to configure Builder / Defaults

    /// <inheritdoc />
    public int IdCounter
    {
        get => _idCounter ??= MyOptions.IdSeed;
        private set => _idCounter = value;
    }
    private int? _idCounter;

    /// <summary>
    /// Fixed date time so all data which receives a default date will have the same value.
    /// </summary>
    private DateTime FixedDateTime { get; } = DateTime.Now;

    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public IContentType ContentType => field
        ??= PctHelper.GetPreferredContentType();
    
    [field: AllowNull, MaybeNull]
    private DataFactoryPreferredContentType PctHelper => field
        ??= new(MyOptions, codeCtManager, typeAssembler, Log);

    /// <summary>
    /// The DataBuilder used for this DataFactory.
    /// </summary>
    /// <remarks>
    /// It's configured using the Options.
    /// So once it's accessed, options cannot be updated anymore.
    /// </remarks>
    [field: AllowNull, MaybeNull]
    private DataAssembler EntityAssemblyKit => field
        ??= dataAssembler.New(new()
        {
            AllowUnknownValueTypes = MyOptions.AllowUnknownValueTypes
        });



    /// <summary>
    /// The relationships which will usually be filled after creating all entities.
    /// They are either a list provided by outside, or a lazy list which will then be filled.
    /// </summary>
    [field: AllowNull, MaybeNull]
    public ILookup<object, IEntity> Relationships => field
        ??= MyOptions.Relationships ?? new LazyLookup<object, IEntity>();

    [field: AllowNull, MaybeNull]
    private RawRelationshipsConvertHelper RelsConvertHelper => field
        ??= new(EntityAssemblyKit.Attribute, Log);

    #endregion

    #region Create IRawEntity / WrapUp

    /// <inheritdoc />
    public IImmutableList<IEntity> Create<T>(IEnumerable<T> list) where T : class, IRawEntitySource
        => WrapUp(Prepare(list));

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

        var cleaned = all
            .OfType<EntityPair<IRawEntity>>()
            .ToListOpt();

        // Verify we don't have nulls (errors)
        if (all.Count == cleaned.Count)
            return l.Return(cleaned);

        // if we have any nulls, take them out and remember the indexes for reporting
        var nullIndexes = all
            .Select((pair, index) => (pair, index))
            .Where(p => p.pair == null)
            .Select(p => p.index)
            .ToListOpt();

        return l.Return(cleaned,
            $"Error preparing: {nullIndexes.Count} items failed to create, indexes: {string.Join(",", nullIndexes)}");
    }

    #endregion

    #region Create basic Dictionary

    /// <inheritdoc />
    public IEntity Create(
        IDictionary<string, object?> values,
        int id = 0,
        Guid guid = default,
        DateTime created = default,
        DateTime modified = default,
        // experimental
        EntityPartsLazy? partsBuilder = default)
    {
        // ID can be created in 3 ways
        // 1. An ID was specified, use that
        // 2. If the ID was 0 / not specified, and the options say to auto-count...
        // 2a. ...the increment from the last count
        // 2b. ...unless the current count is negative, then decrement
        var entityId = id == 0 && MyOptions.AutoId
            ? (IdCounter < 0 ? IdCounter-- : IdCounter++) // negative means we're counting down
            : id;

        // Extra safety check to ensure we don't run into null-issues.
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        values ??= new Dictionary<string, object?>();
        
        // Process possible RawRelationships
        var valuesWithRelationships = RelsConvertHelper.RelationshipsToAttributes(values, Relationships);
        var attributes = EntityAssemblyKit.AttributeList.Finalize(valuesWithRelationships);

        // Create final entity with all the data
        var ent = EntityAssemblyKit.Entity.Create(
            appId: MyOptions.AppId,
            entityId: entityId,
            contentType: ContentType,
            attributes: attributes,
            titleField: MyOptions.TitleField,
            guid: guid,
            created: created == default ? FixedDateTime : created,
            modified: modified == default ? FixedDateTime : modified,
            partsBuilder: partsBuilder
        );
        return ent;
    }

    /// <inheritdoc/>
    public IEntity Create(IRawEntitySource item)
    {
        // Get the raw entity using the extension which checks if it uses a converter or not.
        var raw = item.GetRawFromConverterOrDirectCast(MyOptions.RawConvertOptions);
        return CreateInternal(raw, typeGiver: item);
    }

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

    // #TODO: @2dm #RawEntity - #SpawnNewBadPattern
    public IDataFactory SpawnNew(DataFactoryOptions options)
        => selfGenerator.New(options);
}