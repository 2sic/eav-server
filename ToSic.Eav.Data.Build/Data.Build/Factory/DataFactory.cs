using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Data.Sys.Entities.Sources;

namespace ToSic.Eav.Data.Build;

[PrivateApi("hide implementation")]
[ShowApiWhenReleased(ShowApiMode.Never)]
internal partial class DataFactory(
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

    #region Create basic using values dictionary

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


    #endregion

    // #TODO: @2dm #RawEntity - #SpawnNewBadPattern
    public IDataFactory SpawnNew(DataFactoryOptions options)
        => selfGenerator.New(options);
}