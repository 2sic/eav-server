using ToSic.Eav.Data.ValueConverter.Sys;

namespace ToSic.Eav.Environment.Sys.ServerPaths;

public abstract class ServerPathsBase: IServerPaths
{
    public abstract string FullAppPath(string virtualPath);

    public abstract string FullContentPath(string virtualPath);

    public string FullPathOfReference(string fileReference)
    {
        if (string.IsNullOrWhiteSpace(fileReference)) return fileReference;

        var parts = new LinkParts(fileReference);
        if (parts.IsPage) return fileReference;

        return FullPathOfReference(parts.Id);
    }

    protected abstract string FullPathOfReference(int id);
}