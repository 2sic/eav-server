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
    private bool _allowUnknownValueTypes;

    public DataBuilder New(bool allowUnknownValueTypes = false)
    {
        var clone = builderGen.New();
        clone._allowUnknownValueTypes = allowUnknownValueTypes;
        return clone;
    }

    public ContentTypeBuilder ContentType => contentTypeBuilder.Value;
    public EntityBuilder Entity => entityBuilder.Value;

    public AttributeBuilder Attribute => field ??= attributeBuilder.Value.Setup(_allowUnknownValueTypes);

    public ValueBuilder Value => field ??= valueBuilder.Value.Setup(_allowUnknownValueTypes);

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