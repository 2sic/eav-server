using ToSic.Eav.Data.Sys.Attributes;
using ToSic.Eav.Data.Sys.ContentTypes;

namespace ToSic.Eav.Data.AttributeDefinition.Sys;

/// <summary>
/// WIP attribute model
/// </summary>
//[ModelSpecs(
//    ContentType = MyTypeNameId, // TODO - SHOULD MATCH THE OTHER CONTENT-TYPE
//)]
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

    internal static ContentTypeAttributeAll? FromCodeAttributeOrNull(ContentTypeAttributeSpecsAttribute? attr)
        => attr == null || (attr.Description.IsEmptyOrWs() && attr.InputTypeWIP.IsEmptyOrWs())
            ? null
            : new()
            {
                Description = attr.Description ?? string.Empty,
                InputType = attr.InputTypeWIP ?? string.Empty
            };
}
