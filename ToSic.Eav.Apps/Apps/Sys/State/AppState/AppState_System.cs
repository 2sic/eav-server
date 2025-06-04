using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.Stack;

namespace ToSic.Eav.Apps.State;

partial class AppState
{
    [PrivateApi]
    internal AppStateMetadata SettingsInApp => field ??= new(this, AppStackConstants.Settings);


    [PrivateApi]
    internal AppStateMetadata ResourcesInApp => field ??= new(this, AppStackConstants.Resources);

    [PrivateApi]
    IAppStateMetadata IAppStateCache.ThingInApp(AppThingsToStack target) =>
        target == AppThingsToStack.Settings ? SettingsInApp : ResourcesInApp;
}