using ToSic.Eav.Data.Raw;

namespace ToSic.Eav.Data.ContentTypes;

// todo: rename to ContentTypeModelRaw

[ContentTypeUse(Type = typeof(IContentTypeDetails))]
internal record ContentTypeDetails : IContentTypeDetails, IRawEntityAutoConvert
{
    public string Label { get; init; } = "";

    public string? Description { get; init; }
    
    public string? Notes { get; init; }

    public string? Icon { get; init; }

    public string? Link { get; init; }

    public string? EditInstructions { get; init; }

    public string? ListInstructions { get; init; }

    public string? AdditionalSettings { get; init; }

    public string? DynamicChildrenField { get; init; }
}
