using ToSic.Eav.Apps.Reader;
using ToSic.Lib.Logging;
using static ToSic.Eav.Constants;

namespace ToSic.Eav.Apps;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class AppStatesExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static AppState GetPresetOrNull(this IAppStates states) =>
        (states as AppStates)?.AppsCacheSwitch.Value.Has(PresetIdentity) ?? false
            ? states.GetPresetApp()
            : null;

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static AppState GetPresetApp(this IAppStates states) => states.Get(PresetIdentity);

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static AppState GetPrimaryApp(this IAppStates appStates, int zoneId, ILog log)
    {
        var primaryAppId = appStates.IdentityOfPrimary(zoneId);
        log.A($"{nameof(GetPrimaryApp)}: {primaryAppId?.Show()}");
        return appStates.Get(primaryAppId);
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static int GetPrimaryAppOfAppId(this IAppStates appStates, int appId, ILog log)
    {
        var zoneId = appStates.IdentityOfApp(appId).ZoneId;
        return appStates.GetPrimaryApp(zoneId, log).AppId;
    }

    public static IAppState GetReaderOrNull(this IAppStates appStates, IAppIdentity app) 
        => appStates.GetReaderInternalOrNull(app);

    public static IAppState GetReaderOrNull(this IAppStates appStates, int appId) 
        => appStates.GetReaderInternalOrNull(appId);

    public static IAppStateInternal GetReaderInternalOrNull(this IAppStates appStates, IAppIdentity app)
    {
        var state = appStates.Get(app);
        return state is null ? null : new AppStateReader(state, /*Log */ null);
    }
    public static IAppStateInternal GetReaderInternalOrNull(this IAppStates appStates, int appId)
    {
        var state = appStates.Get(appId);
        return state is null ? null : new AppStateReader(state, /*Log */ null);
    }
}