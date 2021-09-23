using ToSic.Eav.Logging;

namespace ToSic.Eav.Run.Unknown
{
    public sealed class ServerPathsUnknown: IServerPaths, IIsUnknown
    {
        public ServerPathsUnknown(WarnUseOfUnknown<ServerPathsUnknown> warn)
        {

        }

        public string FullAppPath(string virtualPath) => "unknown-path, please implement IServerPaths";

        public string FullContentPath(string virtualPath) => "unknown-path, please implement IServerPaths";
    }
}
