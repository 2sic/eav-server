using ToSic.Eav.Data.Sys.ContentTypes;
using ToSic.Eav.Models;

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

    internal static ContentTypeAttributeAll? FromCodeAttributeOrNull(ContentTypeAttributeSpecsAttribute? attr)
        => attr == null || (attr.Description.IsEmptyOrWs() && attr.InputTypeWIP.IsEmptyOrWs())
            ? null
            : new()
            {
                Description = attr.Description ?? string.Empty,
                InputType = attr.InputTypeWIP ?? string.Empty
            };
}
