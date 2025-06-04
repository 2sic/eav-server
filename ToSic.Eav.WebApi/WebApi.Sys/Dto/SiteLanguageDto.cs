using ToSic.Eav.WebApi.Sys.Security;

namespace ToSic.Eav.WebApi.Sys.Dto;

public class SiteLanguageDto
{
    public string Code { get; set; }

    public string NameId => Code?.ToLowerInvariant();

    public string Culture { get; set; }

    public bool IsEnabled { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsAllowed { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HasPermissionsDto Permissions { get; set; }
}