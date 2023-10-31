using ToSic.Eav.DataSource;
using ToSic.Eav.Services;

namespace ToSic.Eav.Apps.AppSys
{
    public class AppWorkCtxPlus: AppWorkCtx, IAppWorkCtxPlus
    {
        public AppWorkCtxPlus(IDataSourcesService dsf, AppState appState, bool? showDrafts, IDataSource data = default) : base(appState)
        {
            DataSourcesFactory = dsf;
            ShowDrafts = showDrafts;
            _data = data;
        }

        public AppWorkCtxPlus(IAppWorkCtx original, IDataSourcesService dsf = default, AppState appState = default, bool? showDrafts = default, IDataSource data = default) : base(original, appState)
        {
            var origOfClass = original as AppWorkCtxPlus;
            DataSourcesFactory = dsf ?? origOfClass?.DataSourcesFactory;
            ShowDrafts = showDrafts ?? (original as IAppWorkCtxPlus)?.ShowDrafts ?? origOfClass?.ShowDrafts;
            _data = data ?? origOfClass?._data;
        }

        public IAppWorkCtxPlus NewWithPresetData(IDataSource data) => new AppWorkCtxPlus(this, data: data);

        private IDataSourcesService DataSourcesFactory { get; }


        // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
        public IDataSource Data => _data ?? (_data = DataSourcesFactory.CreateDefault(new DataSourceOptions(appIdentity: AppState, showDrafts: ShowDrafts)));
        private IDataSource _data;


        public bool? ShowDrafts { get; }

    }
}
