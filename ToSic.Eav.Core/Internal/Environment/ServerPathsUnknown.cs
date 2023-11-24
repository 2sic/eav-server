using ToSic.Eav.Internal.Unknown;

namespace ToSic.Eav.Internal.Environment;

public sealed class ServerPathsUnknown: ServerPathsBase, IIsUnknown
{
    public ServerPathsUnknown(WarnUseOfUnknown<ServerPathsUnknown> _)
    {

    }

    public override string FullAppPath(string virtualPath) => "unknown-path, please implement IServerPaths";

    public override string FullContentPath(string virtualPath) => "unknown-path, please implement IServerPaths";

    protected override string FullPathOfReference(int id) => $"unknown-path for id {id}, please implement IServerPaths";
}