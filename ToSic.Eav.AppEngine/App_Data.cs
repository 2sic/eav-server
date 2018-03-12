using System;
using System.Linq;
using ToSic.Eav.Apps.Interfaces;
using ToSic.Eav.ValueProvider;

namespace ToSic.Eav.Apps
{
    public partial class App
    {


        #region Data

        /// <summary>
        /// needed to initialize data - must always happen a bit later because the show-draft info isn't available when creating the first App-object.
        /// todo: later this should be moved to initialization of this object
        /// </summary>
        /// <param name="showDrafts"></param>
        /// <param name="versioningEnabled"></param>
        /// <param name="configurationValues">this is needed for providing parameters to the data-query-system</param>
        public void InitData(bool showDrafts, bool versioningEnabled, IValueCollectionProvider configurationValues)
        {
            Log.Add($"init data drafts:{showDrafts}, vers:{versioningEnabled}, hasConf:{configurationValues != null}");
            ConfigurationProvider = configurationValues;
            ShowDraftsInData = showDrafts;
            VersioningEnabled = versioningEnabled;
        }


        public IAppData Data => _data ?? (_data = BuildData());
        private IAppData _data;

        protected virtual DataSources.App BuildData()
        {
            Log.Add("configure on demand start");
            if (ConfigurationProvider == null)
                throw new Exception("Cannot provide Data for the object App as crucial information is missing. " +
                                    "Please call InitData first to provide this data.");

            // ModulePermissionController does not work when indexing, return false for search
            var initialSource = DataSource.GetInitialDataSource(ZoneId, AppId, ShowDraftsInData,
                ConfigurationProvider as ValueCollectionProvider, Log);

            // todo: probably use the full configuration provider from function params, not from initial source?
            var xData = DataSource.GetDataSource<DataSources.App>(initialSource.ZoneId,
                initialSource.AppId, initialSource, initialSource.ConfigurationProvider, Log);

            Log.Add("configure on demand completed");

            GetLanguageAndUser(xData);
            return xData;
        }

        /// <summary>
        /// Override and enhance with environment data like current user, languages, etc.
        /// </summary>
        /// <returns></returns>
        protected void GetLanguageAndUser(DataSources.App xData)
        {
            var languagesActive = Env.ZoneMapper.CulturesWithState(Tenant.Id, ZoneId)
                .Any(c => c.Active);
            xData.DefaultLanguage = languagesActive
                ? Tenant.DefaultLanguage
                : "";
            xData.CurrentUserName = Env.User.CurrentUserIdentityToken;
        }

        #endregion
    }
}
