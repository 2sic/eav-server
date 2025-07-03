using ToSic.Eav.DataSource.VisualQuery.Internal;

namespace ToSic.Eav.DataSource.Sys.AppDataSources;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppDataSourcesLoader
{
    AppLocalDataSources CompileDynamicDataSources(int appId);
}

public record AppLocalDataSources(
    List<DataSourceInfo> Data,
    TimeSpan SlidingExpiration,
    IList<string> FolderPaths,
    IEnumerable<string> CacheKeys
);