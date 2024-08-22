namespace ToSic.Eav.Apps;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class AppStatesExtensions
{
    //[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    //public static IAppContentTypeService GetPresetReaderIfAlreadyLoaded(this IAppStates states) =>
    //    (states as AppStates)?.AppsCacheSwitch.Value.Has(PresetIdentity) ?? false
    //        ? states.GetPresetReader()
    //        : null;

    //[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    //public static IAppStateInternal GetPresetReader(this IAppStates states)
    //    => states.GetReader(PresetIdentity);

    //public static IAppStateInternal GetPrimaryReader(this IAppStates appStates, int zoneId, ILog log)
    //{
    //    var l = log.Fn<IAppStateInternal>($"{zoneId}");
    //    var primaryAppId = appStates.IdentityOfPrimary(zoneId);
    //    return l.Return(appStates.GetReader(primaryAppId, log), primaryAppId.Show());
    //}

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static int GetPrimaryAppOfAppId(this IAppsCatalog appsCatalog, int appId, ILog log)
    {
        var l = log.Fn<int>($"{appId}");
        var zoneId = appsCatalog.AppIdentity(appId).ZoneId;
        var primaryIdentity = appsCatalog.PrimaryAppIdentity(zoneId);
        return l.Return(primaryIdentity.AppId, primaryIdentity.Show());
    }

    //[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    //public static IAppStateCache GetPrimaryAppOfZoneId(this IAppStates appStates, int zoneId, ILog log)
    //{
    //    var l = log.Fn<IAppStateCache>($"{zoneId}");
    //    var primaryIdentity = appStates.AppsCatalog.PrimaryAppIdentity(zoneId);
    //    return l.Return(appStates.Get(primaryIdentity), primaryIdentity.Show());
    //}

    //public static IAppStateInternal GetReader(this IAppStates appStates, IAppIdentity app, ILog log = default)
    //{
    //    var state = appStates.Get(app);
    //    return state is null ? null : appStates.ToReader(state, log);
    //}
    //public static IAppStateInternal GetReader(this IAppStates appStates, int appId, ILog log = default)
    //{
    //    var state = appStates.GetCacheState(appId);
    //    return state is null ? null : appStates.ToReader(state, log);
    //}

    //[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    //public static IAppState KeepOrGetReader(this IAppStates appStates, IAppIdentity app, ILog log = default)
    //    => (app is IAppStateCache stateCache ? appStates.ToReader(stateCache) : null)
    //       ?? app as IAppState ?? appStates.GetReader(app);

    //[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    //public static IAppState KeepOrGetReader(this LazySvc<IAppStates> appStates, IAppIdentity app)
    //    => app as IAppState ?? appStates.Value.GetReader(app);

}