using System.Collections.Generic;
using Newtonsoft.Json;
using ToSic.Eav.WebApi.Security;

namespace ToSic.Eav.WebApi.Dto
{
    public class SiteLanguageDto
    {
        public string Code { get; set; }

        public string NameId => Code?.ToLowerInvariant();

        public string Culture { get; set; }

        public bool IsEnabled { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsAllowed { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public HasPermissionsDto Permissions { get; set; }
    }
}
