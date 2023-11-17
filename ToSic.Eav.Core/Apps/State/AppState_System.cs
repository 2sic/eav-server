using ToSic.Eav.Configuration;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps
{
    public partial class AppState
    {
        /// <summary>
        /// Important: The call to this is a bit ugly, as it must contain a service provider.
        /// This is for cases where the stack hasn't been built yet, in which case it must get 
        /// </summary>
        /// <param name="sp"></param>
        /// <returns></returns>
        [PrivateApi]
        public AppStateMetadata SettingsInApp => _settingsInApp ?? (_settingsInApp = new AppStateMetadata(this, AppStackConstants.Settings));
        private AppStateMetadata _settingsInApp;


        [PrivateApi]
        public AppStateMetadata ResourcesInApp => _resourcesInApp ?? (_resourcesInApp = new AppStateMetadata(this, AppStackConstants.Resources));
        private AppStateMetadata _resourcesInApp;

        [PrivateApi]
        public AppStateMetadata ThingInApp(AppThingsToStack target) =>
            target == AppThingsToStack.Settings ? SettingsInApp : ResourcesInApp;
    }
}
