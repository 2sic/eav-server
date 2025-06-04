using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Data.Build;

[ShowApiWhenReleased(ShowApiMode.Never)]
[method: PrivateApi]
public class DataBuilder(
    LazySvc<EntityBuilder> entityBuilder,
    LazySvc<AttributeBuilder> attributeBuilder,
    LazySvc<ValueBuilder> valueBuilder,
    LazySvc<ContentTypeBuilder> contentTypeBuilder,
    LazySvc<ContentTypeAttributeBuilder> typeAttributeBuilder,
    LazySvc<DimensionBuilder> languageBuilder)
    : ServiceWithSetup<DataBuilderOptions>($"{EavLogs.Eav}MltBld", connect:
        [
            entityBuilder, contentTypeBuilder, attributeBuilder, valueBuilder, typeAttributeBuilder, languageBuilder
        ])
{
    public ContentTypeBuilder ContentType => contentTypeBuilder.Value;
    public EntityBuilder Entity => entityBuilder.Value;

    [field: AllowNull, MaybeNull]
    public AttributeBuilder Attribute => field
        ??= attributeBuilder.Value.Setup(Options.AllowUnknownValueTypes);

    [field: AllowNull, MaybeNull]
    public ValueBuilder Value => field
        ??= valueBuilder.Value.Setup(Options.AllowUnknownValueTypes);

    public ContentTypeAttributeBuilder TypeAttributeBuilder => typeAttributeBuilder.Value;

    public DimensionBuilder Language => languageBuilder.Value;


    public IEntity FakeEntity(int appId)
        => entityBuilder.Value.Create(
            appId: appId,
            attributes: Attribute.Create(new Dictionary<string, object> { { AttributeNames.TitleNiceName, "" } }),
            contentType: ContentType.Transient("FakeEntity"),
            titleField: AttributeNames.TitleNiceName
        );


}