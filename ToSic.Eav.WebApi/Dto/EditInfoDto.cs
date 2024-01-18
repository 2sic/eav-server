using ToSic.Eav.Data.Shared;

namespace ToSic.Eav.WebApi.Dto;

/// <summary>
/// Extends common DTOs to inform about it being read-only, and why
/// </summary>
public class EditInfoDto
{
    public bool ReadOnly { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string ReadOnlyMessage { get; set; }

    /// <summary>
    /// Empty Constructor for deserialization - important
    /// </summary>
    public EditInfoDto() {}


    public EditInfoDto(IContentType contentType)
    {
        // If entity is null it's probably going to be new, so assume it doesn't have an ancestor
        var hasAncestor = contentType?.HasAncestor() ?? false;
        ReadOnly = hasAncestor;
        ReadOnlyMessage = !hasAncestor ? null :
            contentType.HasPresetAncestor() ? "This is a preset ContentType" : "This is an inherited ContentType";
    }

    public EditInfoDto(IEntity entity)
    {
        // If entity is null it's probably going to be new, so assume it doesn't have an ancestor
        var hasAncestor = entity?.HasAncestor() ?? false;
        ReadOnly = hasAncestor;
        ReadOnlyMessage = hasAncestor ? "Item is inherited from another App" : null;
    }
}