using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.ContentTypes;

namespace ToSic.Eav.Data.AttributeDefinition.Sys;

/// <summary>
/// WIP attribute model
/// </summary>
[ContentTypeSpecs(
    Name = MyTypeNameId,
    Guid = "0bab4be8-e795-4d9f-b50e-f7ec161ed8cb",  // must match DB Guid of @All
    Description = "Content-Type for the main properties which 'all' attributes have."
)]
internal record ContentTypeAttributeAll
{
    public const string MyTypeNameId = "@All";

    public string Description { get; init; }
    
    public string InputType { get; init; }

    public Dictionary<string, object?> BuildValues()
    {
        var dic = new Dictionary<string, object?>();
        if (!Description.IsEmptyOrWs())
            dic.Add(AttributeMetadataConstants.DescriptionField, Description);
        if (!InputType.IsEmptyOrWs())
            dic.Add(AttributeMetadataConstants.GeneralFieldInputType, InputType);
        return dic;
    }

    // ContentTypeAttributes
    // AttributeDefinitions

    internal static ContentTypeAttributeAll? FromCodeAttributeOrNull(ContentTypeAttributeSpecsAttribute? attr)
        => attr == null || (attr.Description.IsEmptyOrWs() && attr.InputTypeWIP.IsEmptyOrWs())
            ? null
            : new()
            {
                Description = attr.Description ?? string.Empty,
                InputType = attr.InputTypeWIP ?? string.Empty
            };
}
