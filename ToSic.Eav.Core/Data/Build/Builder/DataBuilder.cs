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
    LazySvc<DimensionBuilder> languageBuilder)
    : ServiceBase(EavLogs.Eav + "MltBld",
        connect:
        [
            entityBuilder, contentTypeBuilder, attributeBuilder, valueBuilder, typeAttributeBuilder, languageBuilder
        ])
{
    public ContentTypeBuilder ContentType => contentTypeBuilder.Value;
    public EntityBuilder Entity => entityBuilder.Value;

    public AttributeBuilder Attribute => attributeBuilder.Value;

    public ValueBuilder Value => valueBuilder.Value;

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