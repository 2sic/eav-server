using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.ContentTypes.Fields;

/// <summary>
/// Content-Type for ...WIP attribute model
/// </summary>
[ContentType(
    Name = MyTypeNameId,
    Guid = "0bab4be8-e795-4d9f-b50e-f7ec161ed8cb",  // made-up GUID, can't match DB Guid since it's currently `@All`
    Description = "General settings for every Attribute (field) on a Content-Type."
)]
internal record FieldSettingsGeneral: RawEntity
{
    public const string MyTypeNameId = "@All";

    public string Notes { get; init; } = "";

    public string InputType { get; init; } = "";

    protected override IDictionary<string, object?> GetValues() =>
        new Dictionary<string, object?>
        {
            { nameof(IFieldSettingsGeneral.Notes), Notes },
            { nameof(IFieldSettingsGeneral.InputType), InputType }
        };

    internal static FieldSettingsGeneral? FromCodeAttributeOrNull(ContentTypeFieldAttribute? attr)
        => attr == null || (attr.Description.IsEmptyOrWs() && attr.InputTypeWIP.IsEmptyOrWs())
            ? null
            : new()
            {
                Notes = attr.Description ?? "",
                InputType = attr.InputTypeWIP ?? ""
            };
}
