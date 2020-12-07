using System.Collections.Generic;
using Newtonsoft.Json;
using ToSic.Eav.WebApi.Security;
using static Newtonsoft.Json.NullValueHandling;

namespace ToSic.Eav.WebApi.Dto
{
    public class ContextDto
    {
        [JsonProperty(NullValueHandling = Ignore)] public AppDto App { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public LanguageDto Language { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public UserDto User { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public WebResourceDto System { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public WebResourceDto Site { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public WebResourceDto Page { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public EnableDto Enable { get; set; }
    }

    public class WebResourceDto
    {
        [JsonProperty(NullValueHandling = Ignore)] public int? Id { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public string Url { get; set; }
    }

    public class AppDto: WebResourceDto
    {
        public string Name { get; set; }
        public string Identifier { get; set; }
        public string GettingStartedUrl { get; set; }
        public string Folder { get; set; }
        public HasPermissionsDto Permissions { get; set; }
    }

    public class EnableDto
    {
        public bool AppPermissions { get; set; }
        public bool CodeEditor { get; set; }
        public bool Query { get; set; }
    }

    public class LanguageDto
    {
        public string Primary { get; set; }
        public string Current { get; set; }
        public Dictionary<string, string> All { get; set; }
    }

    /// <summary>
    /// Will be enhanced later
    /// </summary>
    public class UserDto
    {
    }
}
