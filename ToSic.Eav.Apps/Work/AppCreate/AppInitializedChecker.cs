using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Metadata;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using static ToSic.Eav.Apps.AppLoadConstants;

namespace ToSic.Eav.Apps.Work;

/// <summary>
/// Lightweight tool to check if an app has everything. If not, it will generate all objects needed to then create what's missing.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class AppInitializedChecker : ServiceBase, IAppInitializedChecker
{
    private readonly Generator<AppInitializer> _appInitGenerator;

    #region Constructor / DI

    public AppInitializedChecker(Generator<AppInitializer> appInitGenerator) : base("Eav.AppBld") 
        => ConnectServices(_appInitGenerator = appInitGenerator);

    #endregion

    /// <inheritdoc />
    public bool EnsureAppConfiguredAndInformIfRefreshNeeded(AppState appState, string appName, CodeRefTrail codeRefTrail, ILog parentLog)
    {
        var log = new Log("Eav.AppChk", parentLog);

        var l = log.Fn<bool>($"..., {appName}");

        if (CheckIfAllPartsExist(appState, out _, out _, out _, log))
            return l.ReturnFalse("ok");

        // something is missing, so we must build them
        _appInitGenerator.New().InitializeApp(appState, appName, codeRefTrail.WithHere());

        return l.ReturnTrue();
    }

    /// <summary>
    /// Quickly check if the desired content-types already exist or not
    /// </summary>
    /// <remarks>
    /// This should remain static, because it's used 2x
    /// </remarks>
    /// <param name="appState"></param>
    /// <param name="appConfig"></param>
    /// <param name="appResources"></param>
    /// <param name="appSettings"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    internal static bool CheckIfAllPartsExist(AppState appState, out IEntity appConfig, out IEntity appResources, out IEntity appSettings, ILog log)
    {
        var l = log.Fn<bool>();
        appConfig = appState.GetMetadata(TargetTypes.App, appState.AppId, TypeAppConfig).FirstOrDefault();
        appResources = appState.GetMetadata(TargetTypes.App, appState.AppId, TypeAppResources).FirstOrDefault();
        appSettings = appState.GetMetadata(TargetTypes.App, appState.AppId, TypeAppSettings).FirstOrDefault();


        // if nothing must be done, return now
        if (appConfig != null && appResources != null && appSettings != null)
            return l.ReturnTrue("all ok");

        l.A($"Config: {appConfig != null}, Resources: {appResources != null}, Settings: {appSettings != null}");

        return l.ReturnFalse("some missing");
    }

}