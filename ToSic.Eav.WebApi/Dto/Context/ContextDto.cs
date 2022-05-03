using System.Collections.Generic;
using Newtonsoft.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.WebApi.Security;
using static Newtonsoft.Json.NullValueHandling;

namespace ToSic.Eav.WebApi.Dto
{
    public class ContextDto
    {
        [JsonProperty(NullValueHandling = Ignore)] public ContextAppDto App { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public ContextLanguageDto Language { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public ContextUserDto User { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public ContextResourceWithApp System { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public ContextResourceWithApp Site { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public WebResourceDto Page { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public ContextEnableDto Enable { get; set; }
    }

    public class WebResourceDto
    {
        [JsonProperty(NullValueHandling = Ignore)] public int? Id { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public string Url { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public string SharedUrl { get; set; }
    }

    public class ContextResourceWithApp: WebResourceDto
    {
        [JsonProperty(NullValueHandling = Ignore)] public IAppIdentity DefaultApp { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public IAppIdentity PrimaryApp { get; set; }
    }

    public class ContextAppDto: WebResourceDto
    {
        public string Name { get; set; }
        public string Identifier { get; set; }

        public string SettingsScope { get; set; }

        public string GettingStartedUrl { get; set; }
        public string Folder { get; set; }
        public HasPermissionsDto Permissions { get; set; }
        
        /// <summary>
        /// App API Root. Not 100% correct because it doesn't contain the App identity or anything
        /// But this tells the UI where to go work with things that need the App-API
        /// </summary>
        public string Api { get; set; }

        /// <summary>
        /// Determines if this App is global, meaning it shouldn't have files in the site etc. just global
        /// </summary>
        /// <remarks>New in 13.0</remarks>
        [JsonProperty(NullValueHandling = Ignore)] public bool? IsShared { get; set; }

        /// <summary>
        /// Determines if this app was inherited from another App
        /// </summary>
        [JsonProperty(NullValueHandling = Ignore)]  public bool? IsInherited { get; set; }

        /// <summary>
        /// Marks the App which is global for global settings
        /// </summary>
        public bool IsGlobalApp { get; set; }

        /// <summary>
        /// Marks the App which is for the site settings
        /// </summary>
        public bool IsSiteApp { get; set; }

        /// <summary>
        /// Marks the App which is the Content app
        /// </summary>
        public bool IsContentApp { get; set; }

    }

    public class ContextEnableDto
    {
        [JsonProperty(NullValueHandling = Ignore)] public bool? AppPermissions { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public bool? CodeEditor { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public bool? Query { get; set; }

        [JsonProperty(NullValueHandling = Ignore)] public bool? FormulaSave { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public bool? OverrideEditRestrictions { get; set; }
        [JsonProperty(NullValueHandling = Ignore)] public bool? DebugMode { get; set; }
    }

    public class ContextLanguageDto
    {
        public string Primary { get; set; }
        public string Current { get; set; }
        public List<SiteLanguageDto> List { get; set; }
    }


    /// <summary>
    /// Will be enhanced later
    /// </summary>
    public class ContextUserDto
    {
    }
}
