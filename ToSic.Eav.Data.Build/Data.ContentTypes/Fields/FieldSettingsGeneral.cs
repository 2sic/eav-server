using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.Attributes;

namespace ToSic.Eav.Data.ContentTypes.Fields;

/// <summary>
/// Content-Type for ...WIP attribute model
/// </summary>
[ContentType(
    Name = MyTypeNameId,
    Guid = "0bab4be8-e795-4d9f-b50e-f7ec161ed8cb",  // must match DB Guid of @All
    Description = "General settings for every Attribute (field) on a Content-Type."
)]
internal record FieldSettingsGeneral: RawEntity
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

    internal static FieldSettingsGeneral? FromCodeAttributeOrNull(ContentTypeFieldAttribute? attr)
        => attr == null || (attr.Description.IsEmptyOrWs() && attr.InputTypeWIP.IsEmptyOrWs())
            ? null
            : new()
            {
                Description = attr.Description ?? "",
                InputType = attr.InputTypeWIP ?? ""
            };
}
