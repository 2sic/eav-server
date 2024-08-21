using ToSic.Eav.Apps.State;
using ToSic.Eav.Metadata;
using static ToSic.Eav.Apps.AppLoadConstants;

namespace ToSic.Eav.Apps.Internal.Work;

/// <summary>
/// Lightweight tool to check if an app has everything. If not, it will generate all objects needed to then create what's missing.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppInitializedChecker(Generator<AppInitializer> appInitGenerator) : ServiceBase("Eav.AppBld",
    connect: [appInitGenerator]), IAppInitializedChecker
{
    /// <inheritdoc />
    public bool EnsureAppConfiguredAndInformIfRefreshNeeded(IAppReader appState, string appName, CodeRefTrail codeRefTrail, ILog parentLog)
    {
        var log = new Log("Eav.AppChk", parentLog);
        codeRefTrail.WithHere();

        var l = log.Fn<bool>($"..., {appName}");

        if (CheckIfAllPartsExist(appState, codeRefTrail, out _, out _, out _, log))
            return l.ReturnFalse("ok");

        // something is missing, so we must build them
        appInitGenerator.New().InitializeApp(appState, appName, codeRefTrail.WithHere().AddMessage("Add Requested"));

        return l.ReturnTrue();
    }

    /// <summary>
    /// Quickly check if the desired content-types already exist or not
    /// </summary>
    /// <remarks>
    /// This should remain static, because it's used 2x
    /// </remarks>
    /// <param name="appState"></param>
    /// <param name="codeRefTrail"></param>
    /// <param name="appConfig"></param>
    /// <param name="appResources"></param>
    /// <param name="appSettings"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    internal static bool CheckIfAllPartsExist(IAppState appState, CodeRefTrail codeRefTrail, out IEntity appConfig, out IEntity appResources, out IEntity appSettings, ILog log)
    {
        var l = log.Fn<bool>();
        codeRefTrail.WithHere().AddMessage($"App: {appState.AppId}");
        appConfig = appState.GetMetadata(TargetTypes.App, appState.AppId, TypeAppConfig).FirstOrDefault();
        appResources = appState.GetMetadata(TargetTypes.App, appState.AppId, TypeAppResources).FirstOrDefault();
        appSettings = appState.GetMetadata(TargetTypes.App, appState.AppId, TypeAppSettings).FirstOrDefault();

        var allOk = appConfig != null && appResources != null && appSettings != null;
        var msg = $"Config: {appConfig != null}, Resources: {appResources != null}, Settings: {appSettings != null}; AllOk: {allOk}";
        codeRefTrail.AddMessage(msg);
        l.A(msg);

        if (allOk) return l.ReturnTrue("all ok");

        // if something is missing, add more information to better find out what really went wrong.
        // Because ATM it randomly adds them again when they are already there!
        // Must try catch, to be really sure we don't break anything
        // https://github.com/2sic/2sxc/issues/3203
        try
        {
            codeRefTrail.AddMessage($"App Entity Count: {appState.List.Count}");
            codeRefTrail.AddMessage($"App ContentType Count: {appState.ContentTypes.Count()}");
            codeRefTrail.AddMessage($"App Metadata Count: {appState.Metadata?.Count()}");
            codeRefTrail.AddMessage($"Has Type App Config: {appState.GetContentType(TypeAppConfig)}");
            codeRefTrail.AddMessage($"Has Type App Resources: {appState.GetContentType(TypeAppResources)}");
            codeRefTrail.AddMessage($"Has Type App Settings: {appState.GetContentType(TypeAppSettings)}");
        }
        catch { /* ignore */ }

        return l.ReturnFalse("some missing");
    }

}