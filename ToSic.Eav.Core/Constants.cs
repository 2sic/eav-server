using ToSic.Eav.Apps;
using ToSic.Lib.Documentation;

namespace ToSic.Eav;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class Constants
{
    /// <summary>
    /// Name of the Default App in all Zones
    /// </summary>
    public const string DefaultAppGuid = "Default";

    // TODO: 2DM - CONFUSING - is the app called "Default" or "Content" - this seems to be unclear
    public const string ContentAppName = "Content";
    public const string ContentAppFolder = "Content";
    public const string ErrorAppName = "Error"; // it is name of empty Content app (before content templates are installed)


    // Auto-configure IDs for use in the getting-started dialogs
    public static string ContentAppAutoConfigureId = ContentAppName.ToLowerInvariant();
    public static string AppAutoConfigureId = "app";

    /// <summary>
    /// This is an attempt to generate 2sxc specific IDs. The number schema:
    /// - starts with 251c - 1337 / LEET for "2sic"
    /// - followed by 0000 - to make it very obvious to everybody that it's a manually created guid
    /// - eafe means "eav"
    /// - it then must start with a 2 to indicate a v2 GUID https://en.wikipedia.org/wiki/Universally_unique_identifier#Versions
    /// - we're using that 2 to start "2sxc" which we'll write as 2792 on the phone pad
    /// - The next block is used to create a topic group, 1 is a guid for an app for now
    /// - The last number block is used to write the number, we'll use 1 for the primary-app
    /// </summary>
    public const string PrimaryAppGuid = "251c0000-eafe-2792-0001-000000000001";
    public const string PrimaryAppName = "Primary";
    public const string PrimaryAppFolder = "Primary";

    public const int AppIdEmpty = 0;
    public const string AppNameIdEmpty = "none";

    public const int NullId = -2742;
    public const string NullNameId = "unknown";
    public const int IdNotInitialized = -999;
    public const string UrlNotInitialized = "url-not-initialized";

    public const string CultureSystemKey = "Culture";

    /// <summary>
    /// DataTimeline Operation-Key for Entity-States (Entity-Versioning)
    /// </summary>
    public const string DataTimelineEntityJson = "e";


    [PrivateApi] public static readonly int PresetZoneId = -42;
    [PrivateApi] public static readonly int PresetAppId = -42;
    [PrivateApi] public static readonly string PresetName = "Preset";
    [PrivateApi] public static readonly IAppIdentity PresetIdentity = new AppIdentity(PresetZoneId, PresetAppId);

    #region Experimental, not in use yet / WIP, unsure if we'll use this

    // This is just some internal work, as we're considering also loading some presets
    // from the file system, which are installation specific
    // But ATM we're not doing this yet. 
    [PrivateApi] public static readonly int GlobalPresetAppId = -41;
    [PrivateApi] public static readonly string GlobalPresetName = "Preset-Installation";
    [PrivateApi] public static readonly IAppIdentity GlobalPresetIdentity = new AppIdentity(PresetZoneId, GlobalPresetAppId);
        

    #endregion

    /// <summary>
    /// Default ZoneId. Used if none is specified on the Context.
    /// </summary>
    public static readonly int DefaultZoneId = 1;

    /// <summary>
    /// AppId where MetaData (Entities) are stored.
    /// </summary>
    public static readonly int MetaDataAppId = 1;
    [PrivateApi] public static readonly IAppIdentity GlobalIdentity = new AppIdentity(DefaultZoneId, MetaDataAppId);



    #region special uses of Apps


    #endregion

    public const string DynamicType = "dynamic";

    /// <summary>
    /// A non-existing app. Used to mark Entities which are generated on the fly, to be sure we know they are not real. 
    /// </summary>
    public const int TransientAppId = -9999999;

    /// <summary>
    /// Mark system / preset content types as having a parent, so they don't get used / exported in the wrong places
    /// TODO: rename to Preset...
    /// </summary>
    public const int PresetContentTypeFakeParent = -42000001; // just a very strange, dummy number

    public const string GoUrl = "https://go.2sxc.org";
    public static string GoUrlFor(string code) => $"{GoUrl}/{code}";
}