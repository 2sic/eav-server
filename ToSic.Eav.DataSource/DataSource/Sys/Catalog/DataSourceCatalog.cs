﻿using ToSic.Eav.DataSource.Sys.AppDataSources;
using ToSic.Eav.DataSource.VisualQuery.Sys;
using ToSic.Sys.Caching;

namespace ToSic.Eav.DataSource.Sys.Catalog;

/// <summary>
/// A cache of all DataSource Types - initialized upon first access ever, then static cache.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public partial class DataSourceCatalog(
    IServiceProvider serviceProvider,
    LazySvc<IAppDataSourcesLoader> appDataSourcesLoader,
    MemoryCacheService memoryCacheService)
    : ServiceBase("DS.DsCat", connect: [appDataSourcesLoader, memoryCacheService /*, never! add serviceProvider! - it will cause errors of already-disposed at random places */])
{
    /// <summary>
    /// Create Instance of DataSource to get In- and Out-Streams
    /// </summary>
    /// <param name="dsInfo"></param>
    /// <returns></returns>
    public ICollection<string>? GetOutStreamNames(DataSourceInfo dsInfo)
    {
        var l = Log.Fn<ICollection<string>>();
        try
        {
            // This MUST use Build (not GetService<>) since that will also create objects which are not registered
            var dataSourceInstance = serviceProvider.Build<IDataSource>(dsInfo.Type);

            // skip this if out-connections cannot be queried
            return l.Return(dataSourceInstance.Out.Keys.ToList(), "ok");
        }
        catch
        {
            return l.ReturnNull("error");
        }
    }
}