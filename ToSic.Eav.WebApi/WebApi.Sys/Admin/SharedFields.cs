using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.WebApi.Sys.Admin;

[PrivateApi]
[VisualQuery(
    NiceName = "Shared Fields",
    NameId = "f702cddd-7e58-45ee-aec3-e8986e24c58b",
    NameIds = ["System.SharedFields"],
    Type = DataSourceType.System,
    Audience = Audience.System,
    DataConfidentiality = DataConfidentiality.Internal,
    UiHint = "Shared fields and inheritance relationships used while editing content types"
)]
public class SharedFields : CustomDataSource
{
    [Configuration(Fallback = "0")]
    public int AttributeId => Configuration.GetThis(fallback: 0);

    public SharedFields(
        Dependencies services,
        AppWorkQuick<WorkAttributes> workAttributes,
        Generator<ConvertAttributeToDto> convertAttribute,
        LazySvc<ISysFeaturesService> features)
        : base(services, logName: "Eav.SharedFld", connect: [workAttributes, convertAttribute, features])
    {
        ProvideOutRaw(() => Convert(convertAttribute, workAttributes.New(AppId).GetSharedFields(AttributeId)));
        ProvideOutRaw(
            () => FeatureEnabled(features)
                ? Convert(convertAttribute, workAttributes.New(AppId).GetAncestors(AttributeId))
                : [],
            name: "Ancestors"
        );
        ProvideOutRaw(
            () => FeatureEnabled(features)
                ? Convert(convertAttribute, workAttributes.New(AppId).GetDescendants(AttributeId))
                : [],
            name: "Descendants"
        );
    }

    private IEnumerable<ContentTypeFieldModel> Convert(Generator<ConvertAttributeToDto> convertAttribute, List<PairTypeWithAttribute> fields)
        => convertAttribute.New()
            .Init(AppId, true)
            .Convert(fields)
            .Select(field => new ContentTypeFieldModel(field) { Id = field.Id });

    private bool FeatureEnabled(LazySvc<ISysFeaturesService> features)
        => features.Value.IsEnabled(BuiltInFeatures.ContentTypeFieldsReuseDefinitions);
}
