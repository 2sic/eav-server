using ToSic.Eav.Apps.Reader;
using ToSic.Lib.Logging;
using static ToSic.Eav.Constants;

namespace ToSic.Eav.Apps;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class AppStatesExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IAppStateInternal GetPresetReaderIfAlreadyLoaded(this IAppStates states) =>
        (states as AppStates)?.AppsCacheSwitch.Value.Has(PresetIdentity) ?? false
            ? states.GetPresetReader()
            : null;

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static IAppStateInternal GetPresetReader(this IAppStates states) => states.GetReader(PresetIdentity);

    public static IAppStateInternal GetPrimaryReader(this IAppStates appStates, int zoneId, ILog log)
    {
        var l = log.Fn<IAppStateInternal>($"{zoneId}");
        var primaryAppId = appStates.IdentityOfPrimary(zoneId);
        return l.Return(appStates.GetReader(primaryAppId, log), primaryAppId.Show());
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static int GetPrimaryAppOfAppId(this IAppStates appStates, int appId, ILog log)
    {
        var l = log.Fn<int>($"{appId}");
        var zoneId = appStates.IdentityOfApp(appId).ZoneId;
        var primaryIdentity = appStates.IdentityOfPrimary(zoneId);
        return l.Return(primaryIdentity.AppId, primaryIdentity.Show());
    }

    public static IAppStateInternal GetReader(this IAppStates appStates, IAppIdentity app, ILog log = default)
    {
        var state = appStates.Get(app);
        return state is null ? null : new AppStateReader(state, log);
    }
    public static IAppStateInternal GetReader(this IAppStates appStates, int appId, ILog log = default)
    {
        var state = appStates.GetCacheState(appId);
        return state is null ? null : new AppStateReader(state, log);
    }
}