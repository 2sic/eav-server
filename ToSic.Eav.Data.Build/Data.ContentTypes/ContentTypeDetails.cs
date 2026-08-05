using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.ContentTypes;

[ContentType(
    Name = IContentTypeDetails.Constants.ContentTypeName,
    Guid = "3ef2547d-8a6d-4cc4-91e0-a6396b96f7e7", // Made-up Guid!, real Guid is "ContentType" which would fail! should be fixed some day...
    Description = "Content-Type for the main properties which 'all' attributes have."
)]
internal record ContentTypeDetails : RawEntity, IContentTypeDetails
{
    [ContentTypeField(IsTitle = true)]
    public string Label { get; init; } = "";

    public string? Description { get; init; }
    
    public string? Notes { get; init; }

    public string? Icon { get; init; }

    public string? Link { get; init; }

    public string? EditInstructions { get; init; }

    public string? ListInstructions { get; init; }

    /// <summary>
    /// Lists all names of settings (AppSettings) to load when editing this content-type, as it will be needed in Formulas
    /// </summary>
    public string? AdditionalSettings { get; init; }

    public string? DynamicChildrenField { get; init; }

    protected override IDictionary<string, object?> GetValues() =>
        new Dictionary<string, object?>()
        {
            { nameof(Label), Label },
            { nameof(Description), Description },
            { nameof(Notes), Notes },
            { nameof(Icon), Icon },
            { nameof(Link), Link },
            { nameof(EditInstructions), EditInstructions },
            { nameof(ListInstructions), ListInstructions },
            { nameof(AdditionalSettings), AdditionalSettings },
            { nameof(DynamicChildrenField), DynamicChildrenField }
        };
}
