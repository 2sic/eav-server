﻿using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Caching;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Lib.Helpers;
using static ToSic.Eav.DataSource.DataSourceConstants;

namespace ToSic.Eav.DataSources;

/// <summary>
/// The App Root is the entry point for all data. It takes its data from a hidden AppState Cache.
/// It's implemented as a DataSource so that other DataSources can easily attach to it. <br/>
/// This is also the object returned as the root in any query.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
public class AppRoot : DataSourceBase, IAppRoot
{
    [PrivateApi]
    public AppRoot(IAppReaderFactory appReaders, MyServices services) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.Root")
    {
        _appReaders = appReaders;
        ProvideOut(() => AppReader.List);
        ProvideOut(() => AppReader.GetListPublished(), DataSourceConstantsInternal.StreamPublishedName);
        ProvideOut(() => AppReader.GetListNotHavingDrafts(), DataSourceConstantsInternal.StreamDraftsName);
    }
    private readonly IAppReaderFactory _appReaders;

    public override IDataSourceLink Link => _link.Get(() => new DataSourceLink(null, dataSource: this)
        .AddStream(name: DataSourceConstantsInternal.StreamPublishedName)
        .AddStream(name: DataSourceConstantsInternal.StreamDraftsName));

    private readonly GetOnce<IDataSourceLink> _link = new();

    /// <summary>
    /// Special CacheKey generator for AppRoots, which rely on the state
    /// </summary>
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public new ICacheKeyManager CacheKey => _cacheKey ??= new AppRootCacheKey(this);
    private CacheKey _cacheKey;

    /// <summary>
    /// Get the <see cref="AppReader"/> of this app from the cache.
    /// </summary>
    private IAppReader AppReader => _appReader ??= _appReaders.Get(this);
    private IAppReader _appReader;

    #region Cache-Chain

    /// <inheritdoc />
    public override long CacheTimestamp => AppReader.GetCache().CacheTimestamp;

    /// <inheritdoc />
    public override bool CacheChanged(long dependentTimeStamp) => AppReader.GetCache().CacheChanged(dependentTimeStamp);

    /// <summary>
    /// Combination of the current key and all keys of upstream cached items, to create a long unique key for this context.
    /// </summary>
    /// <returns>Full key containing own partial key and upstream keys.</returns>
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public override string CacheFullKey => CachePartialKey;

    #endregion

}