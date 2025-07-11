﻿using ToSic.Eav.Sys;

#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Context.Sys.Site;

internal class SiteUnknown(WarnUseOfUnknown<SiteUnknown> _) : ISite, IIsUnknown
{
    private const string Unknown = "unknown - please implement the ISite interface to get real values";

    /// <summary>
    /// The unknown zone defaults to 2, as #1 is usually reserved for internal stuff
    /// </summary>
    internal const int UnknownZoneId = 2;

    public int Id { get; private set; } = EavConstants.NullId;

    public string Url => Unknown;

    public string UrlRoot => Unknown;

    public int ZoneId { get; private set; } = UnknownZoneId;

    public string CurrentCultureCode => "en-us";

    public string DefaultCultureCode => "en-us";

    public ISite Init(int siteId, ILog? parentLogOrNull)
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