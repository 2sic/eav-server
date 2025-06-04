using ToSic.Eav.DataSource.VisualQuery.Internal;

namespace ToSic.Eav.DataSource.Internal.AppDataSources;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppDataSourcesLoader
{
    (List<DataSourceInfo> data, TimeSpan slidingExpiration, IList<string> folderPaths, IEnumerable<string> cacheKeys) CompileDynamicDataSources(int appId);
}