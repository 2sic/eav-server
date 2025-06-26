﻿using ToSic.Lib.Wrappers;

namespace ToSic.Eav.Context.Sys.Site;

/// <summary>
/// A tenant in the environment with a reference to the original thing.
/// </summary>
/// <typeparam name="T"></typeparam>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract class Site<T>(string logPrefix) : ServiceBase($"{logPrefix}.Site"), ISite, IWrapper<T>
    where T : class
{
    /// <inheritdoc />
    public abstract ISite Init(int siteId, ILog? parentLogOrNull);

    public virtual T GetContents() => UnwrappedSite;

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    protected virtual T UnwrappedSite { get; set; } = null!;

    /// <inheritdoc />
    public abstract string CurrentCultureCode { get; }

    /// <inheritdoc />
    public abstract string DefaultCultureCode { get; }

    /// <inheritdoc />
    public abstract int Id { get; }

    /// <inheritdoc />
    public abstract string Name { get; }

    public abstract string Url { get; }

    public abstract string UrlRoot { get; }

    /// <inheritdoc />
    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public abstract string AppsRootPhysical { get; }

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public abstract string AppsRootPhysicalFull { get; }

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public abstract string AppAssetsLinkTemplate { get; }

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public abstract string ContentPath { get; }


    public abstract int ZoneId { get; }
}