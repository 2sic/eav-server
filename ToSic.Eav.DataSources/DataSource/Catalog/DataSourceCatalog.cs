using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.DataSource.Catalog;

[PrivateApi]
public partial class DataSourceCatalog: ServiceBase
{
    private readonly LazySvc<IAppDataSourcesLoader> _appDataSourcesLoader;
    private IServiceProvider ServiceProvider { get; }

    public DataSourceCatalog(IServiceProvider serviceProvider, LazySvc<IAppDataSourcesLoader> appDataSourcesLoader) : base("DS.DsCat")
    {
        _appDataSourcesLoader = appDataSourcesLoader;
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Create Instance of DataSource to get In- and Out-Streams
    /// </summary>
    /// <param name="dsInfo"></param>
    /// <returns></returns>
    public ICollection<string> GetOutStreamNames(DataSourceInfo dsInfo)
    {
        var l = Log.Fn<ICollection<string>>();
        try
        {
            // This MUST use Build (not GetService<>) since that will also create objects which are not registered
            var dataSourceInstance = ServiceProvider.Build<IDataSource>(dsInfo.Type);

            // skip this if out-connections cannot be queried
            return l.Return(dataSourceInstance.Out.Keys.ToList(), "ok");
        }
        catch
        {
            return l.ReturnNull("error");
        }
    }
}