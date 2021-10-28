using ToSic.Eav.Logging;

namespace ToSic.Eav.Run.Unknown
{
    public sealed class ServerPathsUnknown: ServerPathsBase, IIsUnknown
    {
        public ServerPathsUnknown(WarnUseOfUnknown<ServerPathsUnknown> warn)
        {

        }

        public override string FullAppPath(string virtualPath) => "unknown-path, please implement IServerPaths";

        public override string FullContentPath(string virtualPath) => "unknown-path, please implement IServerPaths";

        protected override string FullPathOfReference(int id) => $"unknown-path for id {id}, please implement IServerPaths";
    }
}
