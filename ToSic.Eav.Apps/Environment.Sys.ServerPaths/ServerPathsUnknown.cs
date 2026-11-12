#pragma warning disable CS9113 // Parameter is unread.
namespace ToSic.Eav.Environment.Sys.ServerPaths;

[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed class ServerPathsUnknown(WarnUseOfUnknown<ServerPathsUnknown> _): IServerPaths, IIsUnknown
{
    public string FullAppPath(string virtualPath)
        => "unknown-path, please implement IServerPaths";

    public string FullContentPath(string virtualPath)
        => "unknown-path, please implement IServerPaths";

    [return: NotNullIfNotNull("fileReference")]
    public string? FullPathOfReference(string fileReference) => ServerPathsShared.FullPathOfReference(fileReference, FullPathOfReference);

    private string FullPathOfReference(int id)
        => $"unknown-path for id {id}, please implement IServerPaths";
}