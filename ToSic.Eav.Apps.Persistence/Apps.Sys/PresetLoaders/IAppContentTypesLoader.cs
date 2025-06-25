using ToSic.Eav.Data.Sys.Entities.Sources;

namespace ToSic.Eav.Apps.Sys.PresetLoaders;

/// <summary>
/// Minimal state loader - can only load parts that an app can load, so content-types and entities
/// </summary>
public interface IAppContentTypesLoader
{
    /// <summary>
    /// Real constructor, after DI
    /// </summary>
    void Init(IAppReader reader, LogSettings logSettings, string? optionalOverrideAppFolder = default);

    /// <summary>
    /// Get all ContentTypes for specified AppId.
    /// </summary>
    /// <param name="entitiesSource"></param>
    IList<IContentType> ContentTypes(IEntitiesSource entitiesSource);
}