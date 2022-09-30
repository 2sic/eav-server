using System.Collections.Generic;
using System.Text.Json.Serialization;
using ToSic.Eav.Apps;
using ToSic.Eav.WebApi.Security;

namespace ToSic.Eav.WebApi.Dto
{
    public class ContextDto
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ContextAppDto App { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ContextLanguageDto Language { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ContextUserDto User { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ContextResourceWithApp System { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ContextResourceWithApp Site { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public WebResourceDto Page { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public ContextEnableDto Enable { get; set; }
    }

    public class WebResourceDto
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? Id { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string Url { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string SharedUrl { get; set; }
    }

    public class ContextResourceWithApp: WebResourceDto
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public IAppIdentity DefaultApp { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public IAppIdentity PrimaryApp { get; set; }
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
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? IsShared { get; set; }

        /// <summary>
        /// Determines if this app was inherited from another App
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]  public bool? IsInherited { get; set; }

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
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? AppPermissions { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? CodeEditor { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? Query { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? FormulaSave { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? OverrideEditRestrictions { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? DebugMode { get; set; }
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
        public int Id { get; set; }
        public bool IsAnonymous { get; set; }
        public bool IsSystemAdmin { get; set; }

        public bool IsSiteAdmin { get; set; }
    }
}
