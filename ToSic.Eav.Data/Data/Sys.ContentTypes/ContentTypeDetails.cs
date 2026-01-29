using ToSic.Eav.Model;

namespace ToSic.Eav.Data.Sys.ContentTypes;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Added in 13.02
/// IMPORTANT: Don't cache this object, as some info inside it can change during runtime
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
[ModelSource(ContentType = ContentTypeName)]
public record ContentTypeDetails : ModelOfEntity
{
    //public ContentTypeDetails(IEntity entity) : base(entity) { }

    public const string ContentTypeName = "ContentType";

    /// <summary>
    /// The title of the content type.
    /// It does some extra work, because on shared content types the title appears to return empty (for reasons unknown).
    ///
    /// This is mainly important in the UI, where otherwise the title would be defaulted to being the system-name.
    /// </summary>
    public override string Title
    {
        get
        {
            var t = base.Title;
            return !string.IsNullOrWhiteSpace(t) ? t : Get("Label", "");
        }
    }

    public string? Icon => GetThis<string>(null);

    public string? DynamicChildrenField => GetThis<string>(null);

    public string? Description => GetThis<string>(null);

    public string? AdditionalSettings => GetThis<string>(null);
}