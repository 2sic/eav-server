using System.Collections.Generic;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Source;

namespace ToSic.Eav.Internal.Loaders;

/// <summary>
/// Minimal state loader - can only load parts that an app can load, so content-types and entities
/// </summary>
public interface IAppContentTypesLoader
{
    /// <summary>
    /// Real constructor, after DI
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    IAppContentTypesLoader Init(AppState app);

    /// <summary>
    /// Get all ContentTypes for specified AppId.
    /// </summary>
    /// <param name="entitiesSource"></param>
    IList<IContentType> ContentTypes(IEntitiesSource entitiesSource);
}