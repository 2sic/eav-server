using System.Collections.Immutable;

using ToSic.Eav.Data.EntityPair.Sys;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.Entities.Sources;

namespace ToSic.Eav.Data.Build;

[PrivateApi("hide implementation")]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal class DataFactory(Generator<DataBuilder, DataBuilderOptions> dataBuilder, Generator<IDataFactory, DataFactoryOptions> selfGenerator, LazySvc<ContentTypeFactory> ctFactoryLazy)
    : ServiceWithSetup<DataFactoryOptions>("Ds.DatBld", connect: [dataBuilder, selfGenerator, ctFactoryLazy]), IDataFactory
{

    #region Properties to configure Builder / Defaults

    /// <inheritdoc />
    public int IdCounter
    {
        get => _idCounter ??= Options.IdSeed;
        private set => _idCounter = value;
    }
    private int? _idCounter;


    private DateTime Created { get; } = DateTime.Now;
    private DateTime Modified { get; } = DateTime.Now;

    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public IContentType ContentType => field
        ??= Options.Type != null
            ? ctFactoryLazy.Value.Create(Options.Type)
            : DataBuilder.ContentType.Transient(Options.TypeName ?? DataConstants.DataFactoryDefaultTypeName);

    /// <summary>
    /// The DataBuilder used for this DataFactory.
    /// </summary>
    /// <remarks>
    /// It's configured using the Options.
    /// So once it's accessed, options cannot be updated anymore.
    /// </remarks>
    [field: AllowNull, MaybeNull]
    private DataBuilder DataBuilder => field ??= dataBuilder.New(new() {
        AllowUnknownValueTypes = Options.AllowUnknownValueTypes
    });



    /// <summary>
    /// The relationships which will usually be filled after creating all entities.
    /// They are either a list provided by outside, or a lazy list which will then be filled.
    /// </summary>
    [field: AllowNull, MaybeNull]
    public ILookup<object, IEntity> Relationships => field
        ??= Options.Relationships ?? new LazyLookup<object, IEntity>();

    [field: AllowNull, MaybeNull]
    private RawRelationshipsConvertHelper RelsConvertHelper => field
        ??= new(DataBuilder, Log);

    #endregion

    #region Create IRawEntity / WrapUp

    /// <inheritdoc />
    public IImmutableList<IEntity> Create<T>(IEnumerable<T> list) where T : IRawEntity
        => WrapUp(Prepare(list));

    /// <inheritdoc />
    public IImmutableList<IEntity> Create<T>(IEnumerable<IHasRawEntity<T>> list) where T : IRawEntity
        => WrapUp(Prepare(list));

    /// <inheritdoc />
    public IImmutableList<IEntity> WrapUp(IEnumerable<ICanBeEntity> rawList)
    {
        var l = Log.Fn<IImmutableList<IEntity>>();

        // Pre-process relationship keys, so they are added to the lookup
        var list = rawList.ToListOpt();
        if (Relationships is LazyLookup<object, IEntity> lazyRelationships)
            RelsConvertHelper.AddRelationshipsToLookup(list, lazyRelationships, Options.RawConvertOptions);

        // Return entities as Immutable list
        return l.Return(list.Select(set => set.Entity).ToImmutableOpt());
    }

    #endregion

    #region Prepare One

    /// <inheritdoc />
    public EntityPair<T> Prepare<T>(IHasRawEntity<T> withRawEntity) where T: IRawEntity
        => Prepare(withRawEntity.RawEntity);

    /// <inheritdoc />
    public EntityPair<T> Prepare<T>(T rawEntity) where T : IRawEntity
        => new(Create(rawEntity), rawEntity);

    #endregion


    #region Prepare Many

    /// <inheritdoc />
    public IList<EntityPair<T>> Prepare<T>(IEnumerable<IHasRawEntity<T>> data) where T: IRawEntity
        => data.Select(Prepare).ToListOpt();

    /// <inheritdoc />
    public IList<EntityPair<TNewEntity>> Prepare<TNewEntity>(IEnumerable<TNewEntity> list) where TNewEntity : IRawEntity
    {
        var all = list
            .Select(raw =>
            {
                try
                {
                    var newEntity = Create(raw);
                    return new EntityPair<TNewEntity>(newEntity, raw);
                }
                catch
                {
                    // Add null to filter out later and report the indexes
                    return null;
                }

            })
            .ToListOpt();

        // if we have any nulls, take them out and remember the indexes
        if (all.Any(p => p == null))
        {
            var indexes = all
                .Select((pair, index) => (pair, index))
                .Where(p => p.pair == null)
                .Select(p => p.index)
                .ToListOpt();
            var msg = string.Join(",", indexes);
            Log.A($"Error preparing: {indexes.Count} items failed to create, indexes: {msg}");
            return all
                .Where(p => p != null)
                .Cast<EntityPair<TNewEntity>>()
                .ToListOpt();
        }

        return all
            .Cast<EntityPair<TNewEntity>>()
            .ToListOpt();
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
        // pre-process RawRelationships
        values ??= new Dictionary<string, object?>();
        var valuesWithRelationships = RelsConvertHelper.RelationshipsToAttributes(values, Relationships);

        // ID can be created in 3 ways
        // 1. An ID was specified, use that
        // 2. If the ID was 0 / not specified, and the options say to auto-count...
        // 2a. ...the increment from the last count
        // 2b. ...unless the current count is negative, then decrement
        var entityId = id == 0 && Options.AutoId
            ? (IdCounter < 0 ? IdCounter-- : IdCounter++) // negative means we're counting down
            : id;

        var attributes = DataBuilder.Attribute.Create(valuesWithRelationships);
        var ent = DataBuilder.Entity.Create(
            appId: Options.AppId,
            entityId: entityId,
            contentType: ContentType,
            attributes: attributes,
            titleField: Options.TitleField,
            guid: guid,
            created: created == default ? Created : created,
            modified: modified == default ? Modified : modified,
            partsBuilder: partsBuilder
        );
        return ent;
    }


    /// <summary>
    /// Internal create from raw
    /// </summary>
    /// <param name="rawEntity"></param>
    /// <returns></returns>
    public IEntity Create(IRawEntity rawEntity)
    {
        var partsBuilder = Options.WithMetadata && rawEntity is RawEntity { Metadata: not null } typed
            ? new EntityPartsLazy(null, (_, _) => typed.Metadata)
            : null;
        return Create(
            rawEntity.Attributes(Options.RawConvertOptions),
            id: rawEntity.Id,
            guid: rawEntity.Guid,
            created: rawEntity.Created,
            modified: rawEntity.Modified,
            partsBuilder: partsBuilder
        );
    }

    #endregion

    public IDataFactory SpawnNew(DataFactoryOptions options)
        => selfGenerator.New(options);
}