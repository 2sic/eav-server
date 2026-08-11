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
    private readonly GenWorkBasic<WorkAttributes> _workAttributes;
    private readonly Generator<ConvertAttributeToDto> _convertAttribute;
    private readonly LazySvc<ISysFeaturesService> _features;

    [Configuration(Fallback = "0")]
    public int AttributeId => Configuration.GetThis(fallback: 0);

    public SharedFields(
        Dependencies services,
        GenWorkBasic<WorkAttributes> workAttributes,
        Generator<ConvertAttributeToDto> convertAttribute,
        LazySvc<ISysFeaturesService> features)
        : base(services, logName: "Eav.SharedFld", connect: [workAttributes, convertAttribute, features])
    {
        _workAttributes = workAttributes;
        _convertAttribute = convertAttribute;
        _features = features;

        ProvideOutRaw(() => Convert(_workAttributes.New(AppId).GetSharedFields(AttributeId)));
        ProvideOutRaw(() => FeatureEnabled() ? Convert(_workAttributes.New(AppId).GetAncestors(AttributeId)) : [], name: "Ancestors");
        ProvideOutRaw(() => FeatureEnabled() ? Convert(_workAttributes.New(AppId).GetDescendants(AttributeId)) : [], name: "Descendants");
    }

    private IEnumerable<ContentTypeFieldModel> Convert(List<PairTypeWithAttribute> fields) =>
        _convertAttribute.New()
            .Init(AppId, true)
            .Convert(fields)
            .Select(field => new ContentTypeFieldModel(field) { Id = field.Id });

    private bool FeatureEnabled() =>
        _features.Value.IsEnabled(BuiltInFeatures.ContentTypeFieldsReuseDefinitions);
}
