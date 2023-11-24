using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.Logging;
using ToSic.Eav.Run;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Context;

internal class SiteUnknown: ISite, IIsUnknown
{
    private const string Unknown = "unknown - please implement the ISite interface to get real values";

    public SiteUnknown(WarnUseOfUnknown<SiteUnknown> _) { }

    /// <summary>
    /// The unknown zone defaults to 2, as #1 is usually reserved for internal stuff
    /// </summary>
    internal const int UnknownZoneId = 2;

    public int Id { get; private set; } = Constants.NullId;

    public string Url => Unknown;

    public string UrlRoot => Unknown;

    public int ZoneId { get; private set; } = UnknownZoneId;

    public string CurrentCultureCode => "en-us";

    public string DefaultCultureCode => "en-us";

    public ISite Init(int siteId, ILog parentLog)
    {
        Id = siteId;
        ZoneId = siteId;
        return this;
    }

    public string Name => Unknown;
    public string AppsRootPhysical => Unknown;
    public string AppsRootPhysicalFull => Unknown;
    public string AppAssetsLinkTemplate => Unknown;
    public string ContentPath => Unknown;
}