﻿using System;
using ToSic.Eav.Documentation;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.Apps
{
    public partial class App
    {
        [PrivateApi]
        public ILookUpEngine ConfigurationProvider
        {
            get
            {
                if (_configurationProviderBuilt) return _configurationProvider;

                // try deferred initialization of the configuration, 
                // this only works if on initialization a _dataConfigBuilder was provided
                if (_dataConfigurationBuilder != null)
                {
                    var config = _dataConfigurationBuilder.Invoke(this);
                    InitData(config.ShowDrafts, config.Configuration);
                }
                _configurationProviderBuilt = true;
                return _configurationProvider;
            }
        }
        private ILookUpEngine _configurationProvider;
        private bool _configurationProviderBuilt;


        #region Data

        private Func<App, IAppDataConfiguration> _dataConfigurationBuilder;


        /// <summary>
        /// needed to initialize data - must always happen a bit later because the show-draft info isn't available when creating the first App-object.
        /// todo: later this should be moved to initialization of this object
        /// </summary>
        /// <param name="showDrafts"></param>
        /// <param name="configurationValues">this is needed for providing parameters to the data-query-system</param>
        private void InitData(bool showDrafts, ILookUpEngine configurationValues)
        {
            Log.Add($"init data drafts:{showDrafts}, hasConf:{configurationValues != null}");
            _configurationProvider = configurationValues;
            ShowDrafts = showDrafts;
        }

        /// <inheritdoc />
        public IAppData Data => _data ?? (_data = BuildData());
        private IAppData _data;

        [PrivateApi]
        protected virtual AppData BuildData()
        {
            var wrapLog = Log.Call<AppData>();
            if (ConfigurationProvider == null)
                throw new Exception("Cannot provide Data for the object App as crucial information is missing. " +
                                    "Please call InitData first to provide this data.");

            // Note: ModulePermissionController does not work when indexing, return false for search
            var initialSource = DataSourceFactory.GetPublishing(this, ShowDrafts, ConfigurationProvider);

            var xData = DataSourceFactory.GetDataSource<AppData>(initialSource).Init(_dependencies.ZoneMapper, _dependencies.Site);

            return wrapLog("ok", xData);
        }

        #endregion
    }
}
