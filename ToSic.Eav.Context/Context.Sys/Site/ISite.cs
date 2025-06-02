using ToSic.Eav.Apps;

namespace ToSic.Eav.Context;

/// <summary>
/// Describes a tenant - this is what the Environment calls a tenant (like a portal in DNN).
/// Mainly caries the ID and some paths to where the Apps store their files.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ISite: IZoneIdentity, IZoneCultureResolver, ILogShouldNeverConnect
{
    #region Constructor Helper

    /// <summary>
    /// This is a special constructor where the tenant object is re-initialized with a specific tenant id
    /// </summary>
    /// <returns></returns>
    ISite Init(int siteId, ILog parentLog);

    #endregion

    /// <summary>
    /// The Id of the site in systems like DNN and Oqtane.
    /// In DNN this is the same as the `PortalId`, in Oqtane the `SiteId`
    /// </summary>
    int Id { get; }

    /// <summary>
    /// The tenant name for human readability (UIs)
    /// Usually the DNN PortalName
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The root path of the tenant for accessing files using server code
    /// </summary>
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    string AppsRootPhysical { get; }

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    string AppsRootPhysicalFull { get; }

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    string AppAssetsLinkTemplate { get; }

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    string ContentPath { get; }


    // unsure about this
    /// <summary>
    /// The resource specific url, like the one to this page or portal
    /// Should also contain the current / primary protocol, so links can be made with it
    /// </summary>
    string Url { get; }

    /// <summary>
    /// The root identifier of the site, like "2sxc.org" or "2sxc.org/en" - aka Site Alias.
    /// Without protocol like Http etc.
    /// </summary>
    string UrlRoot { get; }
}