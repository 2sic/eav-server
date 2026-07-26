using ToSic.Eav.Models;

namespace ToSic.Eav.Data.ContentTypes.Sys;

/// <summary>
/// Model to read properties of details on a content-type.
/// </summary>
/// <remarks>
/// IMPORTANT: Don't cache this object, as some info inside it can change during runtime
/// 
/// * Added in 13.02
/// * Renamed from `ContentTypeDetails` to `ContentTypeDetailsModel` in v22
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
[ModelSpecs(ContentType = ContentTypeConstants.ContentTypeName)]
internal record ContentTypeDetailsModel: ModelFromEntityBasic, IContentTypeDetails
{
    public string? Notes => GetThis<string>(null);
    public string? Icon => GetThis<string>(null);
    public string? Link => GetThis<string>(null);
    public string? EditInstructions => GetThis<string>(null);
    public string? ListInstructions => GetThis<string>(null);
    public string? DynamicChildrenField => GetThis<string>(null);
    public string Label => GetThis("");
    public string? Description => GetThis<string>(null);
    public string? AdditionalSettings => GetThis<string>(null);
}