namespace ToSic.Eav.Apps.Sys.Loaders;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppLoaderTools
{
    IAppsAndZonesLoader RepositoryLoader(ILog parentLog);
}