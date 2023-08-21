using ToSic.Lib.Logging;
using static ToSic.Eav.Constants;

namespace ToSic.Eav.Apps
{
    public static class AppStatesExtensions
    {
        public static AppState GetPresetOrNull(this IAppStates states) =>
            (states as AppStates)?.AppsCacheSwitch.Value.Has(PresetIdentity) ?? false
                ? states.GetPresetApp()
                : null;

        public static AppState GetPresetApp(this IAppStates states) => states.Get(PresetIdentity);

        public static AppState GetPrimaryApp(this IAppStates appStates, int zoneId, ILog log)
        {
            var primaryAppId = appStates.IdentityOfPrimary(zoneId);
            log.A($"{nameof(GetPrimaryApp)}: {primaryAppId?.Show()}");
            return appStates.Get(primaryAppId);
        }

        public static AppState GetPrimaryAppOfAppId(this IAppStates appStates, int appId, ILog log)
        {
            var zoneId = appStates.IdentityOfApp(appId).ZoneId;
            return appStates.GetPrimaryApp(zoneId, log);
        }
    }
}
