using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources
{
    internal class AppWithParents: DataSource
    {
        private readonly IAppStates _appStates;
        private readonly DataSourceFactory _dataSourceFactory;

        public AppWithParents(MyServices services, DataSourceFactory dataSourceFactory, IAppStates appStates) : base(services, $"{DataSourceConstants.LogPrefix}.ApWPar")
        {
            ConnectServices(
                _dataSourceFactory = dataSourceFactory,
                _appStates = appStates
            );
            Provide(GetList);
        }

        private ImmutableArray<IEntity> GetList() => Log.Func(() =>
        {
            var appState = _appStates.Get(this);
            
            // note: using ShowDrafts = false, fix this if this DS ever becomes usable in VisualQuery
            var initialSource = _dataSourceFactory.GetPublishing(appState); //, true);

            var merge = _dataSourceFactory.GetDataSource<StreamMerge>(initialSource);
            // 2dm 2023-01-22 #maybeSupportIncludeParentApps
                var parent = appState.ParentApp;
                while (parent?.AppState != null)
                {
                    var next = _dataSourceFactory.GetPublishing(parent.AppState);
                    merge.In.Add("App" + parent.AppState.NameId, next.Out.First().Value);
                    parent = parent.AppState.ParentApp;
                }

                return merge.Out.First().Value.List.ToImmutableArray();
        });
    }
}
