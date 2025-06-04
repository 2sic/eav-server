using ToSic.Eav.Apps;
using ToSic.Eav.Services;
using ToSic.Lib.Coding;
using ToSic.Lib.LookUp.Engines;

namespace ToSic.Eav.DataSource;

public static class DataSourcesServiceTestAccessors
{
    public static TDataSource CreateTac<TDataSource>(
        this IDataSourcesService dsf,
        NoParamOrder noParamOrder = default,
        IDataSource? upstream = default,
        IAppIdentity? appIdentity = default,
        ILookUpEngine? configLookUp = default) where TDataSource : IDataSource
        => dsf.Create<TDataSource>(
            attach: upstream,
            options: new DataSourceOptions
            {
                AppIdentityOrReader = appIdentity,
                LookUp = configLookUp,
            }
        );

    public static TDataSource CreateNewTac<TDataSource>(
        this IDataSourcesService dsf,
        NoParamOrder noParamOrder = default,
        IDataSource? upstream = default,
        IAppIdentity? appIdentity = default,
        object? options = default) where TDataSource : IDataSource
        => dsf.Create<TDataSource>(
            attach: upstream,
            options: new DataSourceOptionConverter().Create(new DataSourceOptions
            {
                AppIdentityOrReader = appIdentity,
                LookUp = new LookUpEngine(null),
            }, options)
        );

}