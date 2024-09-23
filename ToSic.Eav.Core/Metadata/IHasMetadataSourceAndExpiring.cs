using ToSic.Eav.Caching;

namespace ToSic.Eav.Metadata;

/// <summary>
/// Marks metadata providers.
/// This is important for things that need a source for their metadata, but won't load it till later.
/// This one also has expiry information.
/// </summary>
[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IHasMetadataSourceAndExpiring: ICacheExpiring
{
    /// <summary>
    /// The metadata source
    /// </summary>
    IMetadataSource MetadataSource { get; }
}

