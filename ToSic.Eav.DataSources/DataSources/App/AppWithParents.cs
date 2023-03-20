﻿using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;

namespace ToSic.Eav.DataSources
{
    internal class AppWithParents: DataSource
    {
        private readonly IDataSourceGenerator<StreamMerge> _mergeGenerator;

        public override int AppId
        {
            get => _appId == 0 ? base.AppId : _appId;
            protected internal set => _appId = value;
        }

        public override int ZoneId
        {
            get => _zoneId == 0 ? base.ZoneId : _zoneId;
            protected internal set => _zoneId = value;
        }

        ///// <summary>
        ///// Indicates whether to show drafts or only Published Entities. 
        ///// </summary>
        //[Configuration(Fallback = QueryConstants.ShowDraftsDefault)]
        //public bool ShowDrafts
        //{
        //    get => Configuration.GetThis(QueryConstants.ShowDraftsDefault);
        //    set => Configuration.SetThis(value);
        //}


        private readonly IAppStates _appStates;
        private readonly IDataSourceFactory _dataSourceFactory;
        private int _appId;
        private int _zoneId;

        public AppWithParents(MyServices services, IDataSourceFactory dataSourceFactory, IAppStates appStates, IDataSourceGenerator<StreamMerge> mergeGenerator) : base(services, $"{DataSourceConstants.LogPrefix}.ApWPar")
        {
            ConnectServices(
                _dataSourceFactory = dataSourceFactory,
                _appStates = appStates,
                _mergeGenerator = mergeGenerator
            );
            ProvideOut(GetList);
        }

        private IImmutableList<IEntity> GetList() => Log.Func(() =>
        {
            var appState = _appStates.Get(this);
            
            var initialSource = _dataSourceFactory.CreateDefault(appIdentity: appState);

            var merge = _mergeGenerator.New(source: initialSource);
            // 2dm 2023-01-22 #maybeSupportIncludeParentApps
            var parent = appState.ParentApp;
            var countRecursions = 0;
            while (parent?.AppState != null && countRecursions++ < 5)
            {
                var next = _dataSourceFactory.CreateDefault(appIdentity: parent.AppState);
                merge.In.Add("App" + parent.AppState.NameId, next.Out.First().Value);
                parent = parent.AppState.ParentApp;
            }

            return merge.Out.First().Value.List.ToImmutableList();
        });
    }
}
