using ToSic.Eav.Models;

namespace ToSic.Eav.Data.ContentTypes.Sys;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// IMPORTANT: Don't cache this object, as some info inside it can change during runtime
/// 
/// * Added in 13.02
/// * Renamed from `ContentTypeDetails` to `ContentTypeDetailsModel` in v22
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
[ModelSpecs(ContentType = ContentTypeConstants.ContentTypeName)]
internal record ContentTypeDetailsModel : ModelFromEntityBasic, IContentTypeDetails
{
    ///// <summary>
    ///// The title of the content type.
    ///// It does some extra work, because on shared content types the title appears to return empty (for reasons unknown).
    /////
    ///// This is mainly important in the UI, where otherwise the title would be defaulted to being the system-name.
    ///// </summary>
    //public override string Title => !string.IsNullOrWhiteSpace(base.Title) ? base.Title : Label;

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