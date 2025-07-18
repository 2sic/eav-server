﻿namespace ToSic.Eav.Apps.Sys.Initializers;

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
    /// <param name="newAppName"></param>
    /// <param name="codeRefTrail"></param>
    /// <param name="parentLog"></param>
    /// <returns></returns>
    bool EnsureAppConfiguredAndInformIfRefreshNeeded(IAppReader appIdentity, string? newAppName, CodeRefTrail codeRefTrail, ILog parentLog);
}