﻿using ToSic.Eav.Apps.Sys.Initializers;
using ToSic.Eav.Metadata;
using static ToSic.Eav.Apps.Sys.AppLoadConstants;

namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Lightweight tool to check if an app has everything. If not, it will generate all objects needed to then create what's missing.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppInitializedChecker(Generator<AppInitializer> appInitGenerator) : ServiceBase("Eav.AppBld",
    connect: [appInitGenerator]), IAppInitializedChecker
{
    /// <inheritdoc />
    public bool EnsureAppConfiguredAndInformIfRefreshNeeded(IAppReader appReader, string? newAppName, CodeRefTrail codeRefTrail, ILog parentLog)
    {
        var log = new Log("Eav.AppChk", parentLog);
        codeRefTrail.WithHere();

        var l = log.Fn<bool>($"..., {newAppName}");

        if (CheckIfAllPartsExist(appReader, codeRefTrail, out _, out _, out _, log))
            return l.ReturnFalse("ok");

        l.A($"Some parts are missing. IsHealthy:{appReader.IsHealthy}");
        if (appReader.IsHealthy)
        {
            l.A("the app is healthy, build missing parts");
            appInitGenerator.New().InitializeApp(appReader, newAppName, codeRefTrail.WithHere().AddMessage("Add Requested"));
        }
        else
        {
            l.A($"the app is not healthy, so we can't add missing parts, HealthMessage:'{appReader.HealthMessage}'");
        }

        return l.ReturnTrue();
    }

    /// <summary>
    /// Quickly check if the desired content-types already exist or not
    /// </summary>
    /// <remarks>
    /// This should remain static, because it's used 2x
    /// </remarks>
    /// <param name="appReader"></param>
    /// <param name="codeRefTrail"></param>
    /// <param name="appConfig"></param>
    /// <param name="appResources"></param>
    /// <param name="appSettings"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    internal static bool CheckIfAllPartsExist(IAppReader appReader, CodeRefTrail codeRefTrail, out IEntity? appConfig, out IEntity? appResources, out IEntity? appSettings, ILog log)
    {
        var l = log.Fn<bool>();
        codeRefTrail.WithHere().AddMessage($"App: {appReader.AppId}");
        var appMd = appReader.Metadata;
        appConfig = appMd.GetMetadata(TargetTypes.App, appReader.AppId, TypeAppConfig).FirstOrDefault();
        appResources = appMd.GetMetadata(TargetTypes.App, appReader.AppId, TypeAppResources).FirstOrDefault();
        appSettings = appMd.GetMetadata(TargetTypes.App, appReader.AppId, TypeAppSettings).FirstOrDefault();

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
            codeRefTrail.AddMessage($"App Entity Count: {appReader.List.Count}");
            codeRefTrail.AddMessage($"App ContentType Count: {appReader.ContentTypes.Count()}");
            codeRefTrail.AddMessage($"App Metadata Count: {appReader.Specs.Metadata?.Count()}");
            codeRefTrail.AddMessage($"Has Type App Config: {appReader.TryGetContentType(TypeAppConfig)}");
            codeRefTrail.AddMessage($"Has Type App Resources: {appReader.TryGetContentType(TypeAppResources)}");
            codeRefTrail.AddMessage($"Has Type App Settings: {appReader.TryGetContentType(TypeAppSettings)}");
        }
        catch { /* ignore */ }

        return l.ReturnFalse("some missing");
    }

}