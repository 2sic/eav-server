using ToSic.Eav.DataSource.VisualQuery.Sys;

namespace ToSic.Eav.DataSource.Sys.AppDataSources;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppDataSourcesLoader
{
    AppLocalDataSources CompileDynamicDataSources(int appId);
}

[ShowApiWhenReleased(ShowApiMode.Never)]
public record AppLocalDataSources(
    List<DataSourceInfo> Data,
    TimeSpan SlidingExpiration,
    IList<string> FolderPaths,
    IEnumerable<string> CacheKeys
);