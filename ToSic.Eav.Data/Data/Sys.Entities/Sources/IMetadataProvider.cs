using ToSic.Eav.Metadata.Sys;
using ToSic.Sys.Caching;

namespace ToSic.Eav.Data.Sys.Entities.Sources;

/// <summary>
/// Describes the "backend" of Metadata, since it can come from:
/// 1. a direct list (like during an import or json load)
/// 2. from an app (the usual case)
/// 3. from a deferred app source (like when the app isn't known yet during init, or it's from another app)
/// </summary>
public interface IMetadataProvider : ICacheExpiring, ICacheDependent
{
    /// <summary>
    /// Direct source of all metadata items, if available.
    /// </summary>
    public DirectEntitiesSource? List { get; }


    public IHasMetadataSourceAndExpiring? LookupSource { get; }
}