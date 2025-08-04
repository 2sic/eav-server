using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.Build;

[ShowApiWhenReleased(ShowApiMode.Never)]
[method: PrivateApi]
public class DataBuilder(
    LazySvc<EntityBuilder> entityBuilder,
    Generator<AttributeBuilder, DataBuilderOptions> attributeBuilder,
    Generator<ValueBuilder, DataBuilderOptions> valueBuilder,
    LazySvc<ContentTypeBuilder> contentTypeBuilder,
    LazySvc<ContentTypeAttributeBuilder> typeAttributeBuilder,
    LazySvc<DimensionBuilder> languageBuilder)
    : ServiceWithSetup<DataBuilderOptions>("DaB.MltBld", connect:
        [
            entityBuilder, contentTypeBuilder, attributeBuilder, valueBuilder, typeAttributeBuilder, languageBuilder
        ])
{
    public ContentTypeBuilder ContentType => contentTypeBuilder.Value;
    public EntityBuilder Entity => entityBuilder.Value;

    [field: AllowNull, MaybeNull]
    public AttributeBuilder Attribute => field
        ??= attributeBuilder.New(Options);

    [field: AllowNull, MaybeNull]
    public ValueBuilder Value => field
        ??= valueBuilder.New(Options);

    public ContentTypeAttributeBuilder TypeAttributeBuilder => typeAttributeBuilder.Value;

    public DimensionBuilder Language => languageBuilder.Value;


    public IEntity FakeEntity(int appId)
        => entityBuilder.Value.Create(
            appId: appId,
            attributes: Attribute.Create(new Dictionary<string, object?> { { AttributeNames.TitleNiceName, "" } }),
            contentType: ContentType.Transient("FakeEntity"),
            titleField: AttributeNames.TitleNiceName
        );


}