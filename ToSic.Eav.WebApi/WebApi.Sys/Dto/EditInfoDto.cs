using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Ancestors;

namespace ToSic.Eav.WebApi.Sys.Dto;

/// <summary>
/// Extends common DTOs to inform about it being read-only, and why
/// </summary>
public class EditInfoDto
{
    public bool ReadOnly { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ReadOnlyMessage { get; set; }

    /// <summary>
    /// Empty Constructor for deserialization - important
    /// </summary>
    public EditInfoDto() {}

    public EditInfoDto(IContentType contentType)
    {
        // If entity is null it's probably going to be new, so assume it doesn't have an ancestor
        var hasAncestor = contentType.HasAncestor();
        var isFileBasedWithNegativeIndex = contentType.RepositoryType != RepositoryTypes.Sql;
        ReadOnly = hasAncestor || isFileBasedWithNegativeIndex;
        ReadOnlyMessage = hasAncestor
            ? contentType!.HasPresetAncestor()
                ? "This is a preset ContentType"
                : "This is an inherited ContentType"
            : isFileBasedWithNegativeIndex
                ? "This is not a Content Type in the DB"
                : null;
    }

    public EditInfoDto(IEntity? entity)
    {
        if (entity == null)
            return;

        // If entity is null it's probably going to be new, so assume it doesn't have an ancestor
        var hasAncestor = entity.HasAncestor();
        var isFileBasedWithNegativeIndex = entity.EntityId < 0;
        ReadOnly = hasAncestor || isFileBasedWithNegativeIndex;
        ReadOnlyMessage = hasAncestor
            ? "Item is inherited from another App"
            : isFileBasedWithNegativeIndex
                ? "Item is not from DB"
                : null;
    }
}