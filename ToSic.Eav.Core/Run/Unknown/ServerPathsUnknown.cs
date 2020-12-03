﻿namespace ToSic.Eav.Run.Unknown
{
    public sealed class ServerPathsUnknown: IServerPaths
    {
        public string FullAppPath(string virtualPath) => "unknown-path, please implement IServerPaths";

        public string FullSystemPath(string virtualPath) => "unknown-path, please implement IServerPaths";

        public string FullContentPath(string virtualPath) => "unknown-path, please implement IServerPaths";
    }
}
