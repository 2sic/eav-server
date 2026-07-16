using ToSic.Eav.Data.Sys;
using ToSic.Eav.Data.Sys.Entities.Sources;

namespace ToSic.Eav.Persistence.File;
public class FileSystemLoaderOptions
{
    public required int AppId { get; init; }

    public required string Path
    {
        get;
        init => field = value.ToSystemPath().TrimEnd('\\', '/') + System.IO.Path.DirectorySeparatorChar;
    }
    public required RepositoryTypes RepoType { get; init; }
    public bool IgnoreMissing { get; init; }
    public IEntitiesSource? EntitiesSource { get; init; }
    public LogSettings LogSettings { get; init; } = new();
}
