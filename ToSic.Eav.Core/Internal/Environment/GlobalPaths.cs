using System;
using ToSic.Eav.Helpers;
using ToSic.Eav.Internal.Configuration;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using static System.IO.Path;

namespace ToSic.Eav.Internal.Environment;

public class GlobalPaths: ServiceBase
{
    #region Constructor / DI

    public GlobalPaths(LazySvc<IServerPaths> serverPaths, LazySvc<IGlobalConfiguration> config): base("Viw.Help")
    {
        ConnectServices(
            _serverPaths = serverPaths,
            _config = config
        );
    }
    private readonly LazySvc<IServerPaths> _serverPaths;
    private readonly LazySvc<IGlobalConfiguration> _config;

    #endregion
        

    /// <summary>
    /// Returns the location where module global folder web assets are stored
    /// </summary>
    public string GlobalPathTo(string path, PathTypes pathType)
    {
        var wrapLog = Log.Fn<string>($"path:{path},pathType:{pathType}");
        var assetPath = Combine(_config.Value.AssetsVirtualUrl.Backslash(), path);
        string assetLocation;
        switch (pathType)
        {
            case PathTypes.Link:
                assetLocation = assetPath.ToAbsolutePathForwardSlash();
                break;
            case PathTypes.PhysRelative:
                assetLocation = assetPath.TrimStart('~').Backslash();
                break;
            case PathTypes.PhysFull:
                assetLocation = _serverPaths.Value.FullAppPath(assetPath).Backslash();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(pathType), pathType, null);
        }
        return wrapLog.ReturnAsOk(assetLocation);
    }
}