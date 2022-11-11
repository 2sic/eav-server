using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// Lightweight tool to check if an app has everything. If not, it will generate all objects needed to then create what's missing.
    /// </summary>
    public interface IAppInitializedChecker
    {
        /// <summary>
        /// Will quickly check if the app is initialized. It uses the App-State to do this.
        /// If it's not configured yet, it will trigger automatic
        /// </summary>
        /// <param name="appIdentity"></param>
        /// <param name="appName"></param>
        /// <param name="parentLog"></param>
        /// <returns></returns>
        bool EnsureAppConfiguredAndInformIfRefreshNeeded(AppState appIdentity, string appName, ILog parentLog);
    }
}
