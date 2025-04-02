using ToSic.Eav.Plumbing;
using ToSic.Lib.Data;

namespace ToSic.Eav.DataSource.VisualQuery;

/// <summary>
/// Custom Attribute for DataSources and use in the VisualQuery Designer.
/// Will add information about help, configuration-content-types etc.
/// Only DataSources which have this attribute will be listed in the designer-tool.
///
/// Read more here: [](xref:NetCode.DataSources.Custom.VisualQueryAttribute)
/// </summary>
[PublicApi]

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class VisualQueryAttribute : Attribute, IHasIdentityNameId
{
    /// <summary>
    /// Empty constructor - necessary, so DocFx includes this attribute in documentation.
    /// </summary>
    [PrivateApi]
    public VisualQueryAttribute() { }

    /// <summary>
    /// A primary type of this source, which determines a default icon + some standard help-text
    /// </summary>
    /// <returns>The type, from the <see cref="DataSourceType"/> enum</returns>
    public DataSourceType Type { get; set; } = DataSourceType.Source;

    /// <summary>
    /// Optional custom icon, based on the icon-names from the Material Icons library.
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// List of in-streams expected by this data-source - will be shown in the UI. Default is empty []. 
    /// </summary>
    public string[] In { get; set; } = null;

    //[PrivateApi]
    //public string[] GetSafeIn() => In ?? Array.Empty<string>();

    /// <summary>
    /// Determine if this data sources can have many out-streams with custom names. Default is false.
    /// </summary>
    /// <returns>True if this data source can also provide other named out-streams, false if it only has the defined list of out-streams.</returns>
    public bool DynamicOut { get; set; } = false;

    public const string OutModeDynamic = "dynamic";
    public const string OutModeFixed = "fixed";
    public const string OutModeMirrorIn = "mirror-in";

    /// <summary>
    /// Provide more options for the Out-Mode, like "mirror-in" 
    /// </summary>
    /// <remarks>
    /// Experimental ca. v19.04
    /// </remarks>
    public string OutMode
    {
        get => field ??= DynamicOut ? OutModeDynamic : OutModeFixed;
        set => field = value;
    }

    public bool DynamicIn
    {
        get => _dynamicIn;
        set 
        {
            _dynamicIn = value;
            _DynamicInWasSet = true;
        }
    }

    // ReSharper disable once InconsistentNaming
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public bool _DynamicInWasSet;
    private bool _dynamicIn = false;

    /// <summary>
    /// The help-link to get help for this data source. The UI will offer a help-button if provided. 
    /// </summary>
    public string HelpLink { get; set; } = "";

    /// <summary>
    /// Should configuration be enabled in the VisualQuery designer?
    /// Is automatically true if ExpectsDataOfType is set.
    /// </summary>
    /// <returns>True if we have a known configuration content-type</returns>
    [PrivateApi("not important to the public")]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public bool EnableConfig => ConfigurationType.HasValue();

    /// <summary>
    /// Name of the content-type used to configure this data-source in the visual-query designer. <br/>
    /// The UI will then open an edit-dialog for this content type. 
    /// _Should be a GUID._
    /// </summary>
    /// <remarks>
    /// Older data sources have a name like "|Config ToSic.Eav.DataSources.App", but that's deprecated
    /// </remarks>
    public string ConfigurationType { get; set; }


    /// <summary>
    /// Nice name shown in the UI <br/>
    /// If not specified, the UI will use the normal name instead.
    /// May contain spaces, slashes etc.
    /// </summary>
    public string NiceName { get; set; }


    /// <summary>
    /// A hint to help the user better understand what this does - in case the nice name isn't enough. 
    /// </summary>
    public string UiHint { get; set; }

    /// <summary>
    /// **required** this should be a unique id, ideally a GUID. <br/>
    /// </summary>
    /// <remarks>
    /// * _important: old code use string names like a.net namespace. This should not be done any more and will be deprecated in future._
    /// * Was renamed in 15.04 from `GlobalName` to the new `NameId` convention.
    /// </remarks>
    public string NameId { get; set; } = "";

    /// <summary>
    /// Names this DataSource may have had previously. <br/>
    /// This was introduced when we standardized the names, and still had historic data using old names or old namespaces. 
    /// </summary>
    /// <remarks>
    /// * Was renamed in 15.04 to `NameIds` from `PreviousNames`
    /// </remarks>
    public string[] NameIds { get; set; } = [];

    /// <summary>
    /// This marks the audience of a DataSource.
    /// Specifically it allows hiding advanced DataSources from users who don't need them.
    /// </summary>
    /// <remarks>
    /// Previously hidden/unofficial and called `Difficulty`.
    /// Made public and renamed to `Audience` in v15.03
    /// </remarks>
    public Audience Audience { get; set; } = Audience.Default;
}