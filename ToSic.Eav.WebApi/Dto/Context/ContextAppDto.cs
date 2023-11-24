using System.Text.Json.Serialization;
using ToSic.Eav.WebApi.Security;
using static System.Text.Json.Serialization.JsonIgnoreCondition;

namespace ToSic.Eav.WebApi.Dto;

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
    [JsonIgnore(Condition = WhenWritingNull)] public bool? IsShared { get; set; }

    /// <summary>
    /// Determines if this app was inherited from another App
    /// </summary>
    [JsonIgnore(Condition = WhenWritingNull)]  public bool? IsInherited { get; set; }

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


    [JsonIgnore(Condition = WhenWritingNull)]
    public string Icon { get; set; }
}