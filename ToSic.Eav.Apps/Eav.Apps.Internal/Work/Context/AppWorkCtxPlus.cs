﻿using ToSic.Eav.Apps.State;
using ToSic.Eav.DataSource;
using ToSic.Eav.Services;

namespace ToSic.Eav.Apps.Internal.Work;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppWorkCtxPlus : AppWorkCtx, IAppWorkCtxPlus
{
    //public AppWorkCtxPlus(IDataSourcesService dsf, AppState appState, bool? showDrafts, IDataSource data = default) : base(appState)
    //{
    //    DataSourcesFactory = dsf;
    //    ShowDrafts = showDrafts;
    //    _data = data;
    //}
    public AppWorkCtxPlus(IDataSourcesService dsf, IAppState appState, bool? showDrafts, IDataSource data = default) : base(appState)
    {
        DataSourcesFactory = dsf;
        ShowDrafts = showDrafts;
        _data = data;
    }

    //public AppWorkCtxPlus(IAppWorkCtx original, IDataSourcesService dsf = default, AppState appState = default, bool? showDrafts = default, IDataSource data = default) : base(original, appState)
    //{
    //    var origOfClass = original as AppWorkCtxPlus;
    //    DataSourcesFactory = dsf ?? origOfClass?.DataSourcesFactory;
    //    ShowDrafts = showDrafts ?? (original as IAppWorkCtxPlus)?.ShowDrafts ?? origOfClass?.ShowDrafts;
    //    _data = data ?? origOfClass?._data;
    //}
    public AppWorkCtxPlus(IAppWorkCtx original, IDataSourcesService dsf = default, IAppStateInternal appState = default, bool? showDrafts = default, IDataSource data = default) : base(original, appState)
    {
        var origOfClass = original as AppWorkCtxPlus;
        DataSourcesFactory = dsf ?? origOfClass?.DataSourcesFactory;
        ShowDrafts = showDrafts ?? (original as IAppWorkCtxPlus)?.ShowDrafts ?? origOfClass?.ShowDrafts;
        _data = data ?? origOfClass?._data;
    }

    public IAppWorkCtxPlus NewWithPresetData(IDataSource data) => new AppWorkCtxPlus(this, data: data);

    private IDataSourcesService DataSourcesFactory { get; }


        
    public IDataSource Data => _data ??= DataSourcesFactory.CreateDefault(new DataSourceOptions(appIdentity: AppState.StateCache, showDrafts: ShowDrafts));
    private IDataSource _data;


    public bool? ShowDrafts { get; }

}