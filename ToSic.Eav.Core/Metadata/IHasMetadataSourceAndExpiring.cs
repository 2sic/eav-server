using ToSic.Eav.Caching;
using ToSic.Lib.Caching;

namespace ToSic.Eav.Metadata;

/// <summary>
/// Marks metadata providers.
/// This is important for things that need a source for their metadata, but won't load it till later.
/// This one also has expiry information.
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IHasMetadataSourceAndExpiring: ICacheExpiring
{
    /// <summary>
    /// The metadata source
    /// </summary>
    IMetadataSource MetadataSource { get; }
}

