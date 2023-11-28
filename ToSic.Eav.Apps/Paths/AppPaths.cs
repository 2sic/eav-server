using System;
using ToSic.Eav.Context;
using ToSic.Eav.Data.PiggyBack;
using ToSic.Eav.Helpers;
using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Internal.Environment;
using ToSic.Lib.Logging;
using ToSic.Lib.DI;
using ToSic.Lib.Services;
using static System.IO.Path;



namespace ToSic.Eav.Apps.Paths;

/// <summary>
/// Find the App Paths for a specific App in a site
/// </summary>
/// <remarks>
/// IMPORTANT
/// * There is a complex problem related to Dnn which can have a module in another site.
///   This can lead to an issue where the ISite used here may be the wrong one.
/// * The reason is that the ISite is shared across modules
///   Which is difficult because it's initialized on module_load,
///   But rendering will happen later, and in between other module do the _Load which can change the site
/// * If that happens on the initial call, the cache will be wrong forever.
///   This is why as a temporary workaround we use the SiteId as part of the cache key.
///   Not sexy, but I guess better than alternatives for now.
/// * Future: We should find a way to scope some DI to a module, so it doesn't bleed to others
///   But that is a bit difficult, because there are also some services like the IPage which should be shared across modules
/// </remarks>
internal class AppPaths: ServiceBase, IAppPathsMicroSvc
{
    private const bool Debug = true;

    public AppPaths(LazySvc<IServerPaths> serverPaths, LazySvc<IGlobalConfiguration> config) : base($"{EavLogs.Eav}.AppPth")
    {
        ConnectServices(
            _serverPaths = serverPaths,
            _config = config
        );
    }

    private readonly LazySvc<IServerPaths> _serverPaths;
    private readonly LazySvc<IGlobalConfiguration> _config;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="site">The site - in some cases the site of the App can be different from the context-site, so it must be passed in</param>
    /// <param name="appState"></param>
    /// <returns></returns>
    public IAppPaths Init(ISite site, AppState appState) => Init(site, appState.ToInterface(Log));

    public IAppPaths Init(ISite site, IAppState appState)
    {
        _site = site;
        _appState = appState;
        InitDone = true;
        return this;
    }
    private ISite _site;
    private IAppState _appState;
    public bool InitDone { get; private set; }

    /// <summary>
    /// We are having some difficulties that the App is caching the wrong path, so temporarily we'll log
    /// when we are actually picking up the value to put into the AppState
    /// </summary>
    private void LogAppPathDetails(string property, string result) => Log.Do(l =>
    {
        l.A($"App State: {_appState.Show()}");
        l.A($"Site: {_site.Id}; Zone: {_site.ZoneId};");
        l.A($"{property}: {result}");
    });

    private string InterceptAndLog(string name, string result)
    {
        if (Debug) Log.A($"Intercept AppPath {name}: {result}");
        return result;
    }

    private string GetInternal(string name, Func<string> callIfNotFound)
    {
        // 2022-02-07 2dm try to drop special case with site-id again, as we shouldn't need this any more
        var key = name; // + _site.Id;
        var final = _appState.Internal().GetPiggyBack(key,
            () =>
            {
                var result = callIfNotFound();
                if (Debug) LogAppPathDetails(nameof(Path), result);
                return result;
            });
        if (Debug) InterceptAndLog(name, final);
        return final;
    }

    public string Path => GetInternal(nameof(Path), 
        () => _site.AppAssetsLinkTemplate.Replace(AppConstants.AppFolderPlaceholder, _appState.Folder)
            .ToAbsolutePathForwardSlash());

    public string PathShared => GetInternal(nameof(PathShared), 
        () => Combine(_config.Value.SharedAppsFolder, _appState.Folder)
            .ToAbsolutePathForwardSlash());

    public string PhysicalPath => GetInternal(nameof(PhysicalPath), 
        () => Combine(_site.AppsRootPhysicalFull, _appState.Folder));

    public string PhysicalPathShared => GetInternal(nameof(PhysicalPathShared), 
        () => _serverPaths.Value.FullAppPath(Combine(_config.Value.SharedAppsFolder, _appState.Folder)));

    public string RelativePath => GetInternal(nameof(RelativePath), 
        () => Combine(_site.AppsRootPhysical, _appState.Folder).Backslash());
        
    public string RelativePathShared => GetInternal(nameof(RelativePathShared), 
        () => Combine(_config.Value.SharedAppsFolder, _appState.Folder)
            .ToAbsolutePathForwardSlash());
}