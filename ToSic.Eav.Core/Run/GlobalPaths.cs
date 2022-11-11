﻿using System;
using ToSic.Eav.Configuration;
using ToSic.Eav.Helpers;
using ToSic.Lib.Logging;
using static System.IO.Path;

namespace ToSic.Eav.Run
{
    public class GlobalPaths: HasLog
    {
        #region Constructor / DI

        public GlobalPaths(Lazy<IServerPaths> serverPaths, Lazy<IGlobalConfiguration> config): base("Viw.Help")
        {
            _serverPaths = serverPaths;
            _config = config;
        }
        private readonly Lazy<IServerPaths> _serverPaths;
        private readonly Lazy<IGlobalConfiguration> _config;

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
}