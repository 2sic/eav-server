using System;
using ToSic.Eav.Documentation;
using ToSic.Eav.Plumbing;

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
        public AppStateSettings SettingsInApp(IServiceProvider sp = null) => _settingsInApp ?? (_settingsInApp = new AppStateSettings(this, sp.Build<IAppStates>()));
        private AppStateSettings _settingsInApp;
    }
}
