using ToSic.Eav.Apps.Sys.FileSystemState;
using ToSic.Eav.Data.Sys.Entities.Sources;

namespace ToSic.Eav.Apps.Sys.PresetLoaders;

/// <summary>
/// Minimal state loader - can only load parts that an app can load, so content-types and entities
/// </summary>
public interface IAppContentTypesLoader: IServiceWithSetup<AppFileSystemLoaderOptions>
{
    /// <summary>
    /// Get all ContentTypes for specified AppId.
    /// </summary>
    /// <param name="entitiesSource"></param>
    PartialData TypesAndEntities(IEntitiesSource entitiesSource);
}