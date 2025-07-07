﻿using ToSic.Eav.Apps;
using ToSic.Eav.LookUp.Sys.Engines;
using ToSic.Eav.Services;

namespace ToSic.Eav.DataSource;

public class DataSourcesTstBuilder(IDataSourcesService dataSourcesService): ServiceBase("Tst.DsFHlp", connect: [dataSourcesService])
{
    public IDataSourcesService DataSourceSvc => dataSourcesService;

    public T CreateDataSource<T>(ILookUpEngine lookUpEngine = default) where T : IDataSource
        => dataSourcesService.CreateTac<T>(appIdentity: new AppIdentity(0, 0), configLookUp: lookUpEngine ?? new LookUpEngine(Log));

    public T CreateDataSource<T>(IDataSource upstream) where T : IDataSource
        => dataSourcesService.CreateTac<T>(upstream: upstream);

    public T CreateDataSource<T>(IDataStream upstream, object options = default) where T : IDataSource
        => ((DataSourcesService)dataSourcesService).Create<T>(stream: upstream, new DataSourceOptionConverter().Convert(options, false, false));

    public T CreateDataSource<T>(IDataSourceLinkable attach, object options = default) where T : IDataSource
        => dataSourcesService.Create<T>(attach: attach, new DataSourceOptionConverter().Convert(options, false, false));



    public T CreateDataSourceNew<T>(ILookUpEngine lookUpEngine = default, object options = default) where T : IDataSource
        => dataSourcesService.CreateTac<T>(appIdentity: new AppIdentity(0, 0), configLookUp: lookUpEngine ?? new LookUpEngine(Log));

    public T CreateDataSourceNew<T>(object? options = default) where T : IDataSource
        => dataSourcesService.CreateNewTac<T>(appIdentity: new AppIdentity(0, 0), options: options);

}