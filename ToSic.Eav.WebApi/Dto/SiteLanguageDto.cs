using System.Text.Json.Serialization;
using ToSic.Eav.WebApi.Security;

namespace ToSic.Eav.WebApi.Dto
{
    public class SiteLanguageDto
    {
        public string Code { get; set; }

        public string NameId => Code?.ToLowerInvariant();

        public string Culture { get; set; }

        public bool IsEnabled { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? IsAllowed { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public HasPermissionsDto Permissions { get; set; }
    }
}
