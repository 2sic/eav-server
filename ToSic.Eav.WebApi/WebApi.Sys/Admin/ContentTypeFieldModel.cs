using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Admin;

// TODO: This should be reworked, probably make the DTO the real content-type definer, and just add GetValues?

[ContentType(
    Name = "Content Type Field",
    Guid = "40ef450f-6180-42e7-9c14-8d5411873abb",
    Description = "Field definition used while editing content types",
    Scope = "System"
)]
public record ContentTypeFieldModel(ContentTypeFieldDto field) : RawEntity
{
    protected override IDictionary<string, object?> GetValues() =>
        new Dictionary<string, object?>
        {
            { nameof(field.SortOrder), field.SortOrder },
            { nameof(field.Type), field.Type },
            { nameof(field.InputType), field.InputType },
            { nameof(field.StaticName), field.StaticName },
            { nameof(field.IsTitle), field.IsTitle },
            { nameof(field.AttributeId), field.AttributeId },
            { nameof(field.Metadata), field.Metadata },
            { nameof(field.InputTypeConfig), field.InputTypeConfig },
            { nameof(field.Permissions), field.Permissions },
            { nameof(field.ImageConfiguration), field.ImageConfiguration },
            { nameof(field.IsEphemeral), field.IsEphemeral },
            { nameof(field.HasFormulas), field.HasFormulas },
            { nameof(field.EditInfo), field.EditInfo },
            { nameof(field.Guid), field.Guid },
            { nameof(field.SysSettings), field.SysSettings },
            { nameof(field.ContentType), field.ContentType },
            { nameof(field.ConfigTypes), field.ConfigTypes },
        };
}
