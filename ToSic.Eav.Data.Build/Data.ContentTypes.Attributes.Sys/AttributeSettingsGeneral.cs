using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.ContentTypes;

namespace ToSic.Eav.Data.ContentTypes.Attributes.Sys;

/// <summary>
/// WIP attribute model
/// </summary>
[ContentTypeSpecs(
    Name = MyTypeNameId,
    Guid = "0bab4be8-e795-4d9f-b50e-f7ec161ed8cb",  // must match DB Guid of @All
    Description = "Content-Type for the main properties which 'all' attributes have."
)]
internal record AttributeSettingsGeneral: RawEntity
{
    public const string MyTypeNameId = "@All";

    public string Description { get; init; } = "";

    public string InputType { get; init; } = "";

    protected override IDictionary<string, object?> GetValues() =>
        new Dictionary<string, object?>
        {
            { AttributeMetadataConstants.DescriptionField, Description },
            { AttributeMetadataConstants.GeneralFieldInputType, InputType }
        };

    // ContentTypeAttributes
    // AttributeDefinitions

    internal static AttributeSettingsGeneral? FromCodeAttributeOrNull(ContentTypeAttributeSpecsAttribute? attr)
        => attr == null || (attr.Description.IsEmptyOrWs() && attr.InputTypeWIP.IsEmptyOrWs())
            ? null
            : new()
            {
                Description = attr.Description ?? "",
                InputType = attr.InputTypeWIP ?? ""
            };
}
