using ToSic.Eav.Logging;
using static ToSic.Eav.Constants;

namespace ToSic.Eav.Apps
{
    public static class AppStatesExtensions
    {
        public static AppState GetPresetOrNull(this IAppStates states) =>
            (states as AppStates)?.AppsCacheSwitch.Value.Has(PresetIdentity) ?? false
                ? states.Get(PresetIdentity)
                : null;

        public static AppState GetPresetApp(this IAppStates states) => states.Get(PresetIdentity);

        public static AppState GetPrimaryApp(this IAppStates appStates, int zoneId, ILog loggerOrNull)
        {
            var primaryAppId = appStates.IdentityOfPrimary(zoneId);
            loggerOrNull.A($"{nameof(GetPrimaryApp)}: {primaryAppId?.Show()}");
            return appStates.Get(primaryAppId);
        }
    }
}
