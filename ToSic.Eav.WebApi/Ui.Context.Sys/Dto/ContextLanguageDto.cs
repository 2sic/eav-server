using ToSic.Eav.Context.Sys;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Eav.Data.ContentTypes.Sys;

namespace ToSic.Eav.Ui.Context.Sys.Dto;

public record ContextLanguageDto()
{
    [SetsRequiredMembers]
    public ContextLanguageDto(AppLanguageState state): this()
    {
        Code = state.Code;
        Culture = state.Culture;
        IsAllowed = state.IsAllowed;
        IsEnabled = state.IsEnabled;
        Permissions = null;
    }

    public required string Code { get; init; }

    public string NameId => Code.ToLowerInvariant();

    [ContentTypeTitle]
    public required string Culture { get; init; }

    public required bool IsEnabled { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required bool? IsAllowed { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required HasPermissionsDto? Permissions { get; init; }
}
