using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Build;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
[method: PrivateApi]
public class DataBuilder(
    LazySvc<EntityBuilder> entityBuilder,
    LazySvc<AttributeBuilder> attributeBuilder,
    LazySvc<ValueBuilder> valueBuilder,
    LazySvc<ContentTypeBuilder> contentTypeBuilder,
    LazySvc<ContentTypeAttributeBuilder> typeAttributeBuilder,
    LazySvc<DimensionBuilder> languageBuilder,
    Generator<DataBuilder> builderGen)
    : ServiceBase(EavLogs.Eav + "MltBld",
        connect:
        [
            entityBuilder, contentTypeBuilder, attributeBuilder, valueBuilder, typeAttributeBuilder, languageBuilder, builderGen
        ])
{
    /// <summary>
    /// WIP, should replace the New below...
    /// </summary>
    [field: AllowNull, MaybeNull]
    public DataBuilderOptions Options
    {
        get => field ??= new();
        set => field = field == null
            ? value
            : throw new InvalidOperationException("Options can only be set once, and only when the DataBuilder is created.");
    }

    public ContentTypeBuilder ContentType => contentTypeBuilder.Value;
    public EntityBuilder Entity => entityBuilder.Value;

    public AttributeBuilder Attribute => field ??= attributeBuilder.Value.Setup(Options.AllowUnknownValueTypes);

    public ValueBuilder Value => field ??= valueBuilder.Value.Setup(Options.AllowUnknownValueTypes);

    public ContentTypeAttributeBuilder TypeAttributeBuilder => typeAttributeBuilder.Value;

    public DimensionBuilder Language => languageBuilder.Value;


    public IEntity FakeEntity(int appId)
        => entityBuilder.Value.Create(
            appId: appId,
            attributes: Attribute.Create(new Dictionary<string, object> { { Attributes.TitleNiceName, "" } }),
            contentType: ContentType.Transient("FakeEntity"),
            titleField: Attributes.TitleNiceName
        );



}