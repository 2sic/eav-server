using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Persistence.File;
public class FileSystemLoaderOptions
{
    public required int appId { get; init; }

    public required string path
    {
        get;
        init => field = value + (value.EndsWith("\\") ? "" : "\\");
    }
    public required RepositoryTypes repoType { get; init; }
    public bool ignoreMissing { get; init; }
    public IEntitiesSource? entitiesSource { get; init; }
    public LogSettings logSettings { get; init; } = new();
}
