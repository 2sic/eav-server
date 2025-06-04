namespace ToSic.Eav;

[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class Constants
{
    public const int NullId = -2742;
    public const string NullNameId = "unknown";
    public const int IdNotInitialized = -999;
    public const string UrlNotInitialized = "url-not-initialized";

    public const string CultureSystemKey = "Culture";

    /// <summary>
    /// History Operation-Key for Entity-States (Entity-Versioning)
    /// </summary>
    public const string HistoryEntityJson = "e";

    /// <summary>
    /// Mark system / preset content types as having a parent, so they don't get used / exported in the wrong places
    /// </summary>
    public const int PresetContentTypeFakeParent = -42000001; // just a very strange, dummy number

    public const string GoUrl = "https://go.2sxc.org";
    public const string GoUrlSysFeats = $"{GoUrl}/sysfeats";

    /// <summary>
    /// ApiController files and classes suffix
    /// </summary>
    public const string ApiControllerSuffix = "Controller";

    /// <summary>
    /// ApiController folder and route segment
    /// </summary>
    public const string Api = "api";

    public const string EmptyRelationship = "null";
}