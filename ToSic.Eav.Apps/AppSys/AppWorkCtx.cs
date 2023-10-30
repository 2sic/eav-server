using System;
using ToSic.Eav.DataSource;
using ToSic.Eav.Services;

namespace ToSic.Eav.Apps.AppSys
{
    /// <summary>
    /// Context object for performing App modifications.
    /// This should help us change all the Read/Parts etc. to be fully functional and not depend on a Parent object. 
    /// </summary>
    public class AppWorkCtx : IAppIdentity, IAppWorkCtx
    {
        /// <inheritdoc />
        public int ZoneId { get; }

        /// <inheritdoc />
        public int AppId { get; }


        public AppWorkCtx(IDataSourcesService dsf, AppState appState, bool? showDrafts, IDataSource data = default)
        {
            DataSourcesFactory = dsf;
            AppId = appState.AppId;
            ZoneId = appState.ZoneId;
            AppState = appState;
            ShowDrafts = showDrafts;
            _data = data;
        }

        public AppWorkCtx(IAppWorkCtx original, IDataSourcesService dsf = default, AppState appState = default, bool? showDrafts = default, IDataSource data = default)
        {
            if (original == null) throw new ArgumentException(@"Original must exist", nameof(original));
            var origOfClass = original as AppWorkCtx;
            DataSourcesFactory = dsf ?? origOfClass?.DataSourcesFactory;
            AppId = appState?.AppId ?? original.AppId;
            ZoneId = appState?.ZoneId ?? original.ZoneId;
            AppState = appState ?? original.AppState;
            ShowDrafts = showDrafts ?? original.ShowDrafts;
            _data = data ?? origOfClass?._data;
        }

        public IDataSourcesService DataSourcesFactory { get; }

        public AppState AppState { get; }

        // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
        public IDataSource Data => _data ?? (_data = DataSourcesFactory.CreateDefault(new DataSourceOptions(appIdentity: this, showDrafts: ShowDrafts)));
        private IDataSource _data;


        public bool? ShowDrafts { get; }
    }
}
