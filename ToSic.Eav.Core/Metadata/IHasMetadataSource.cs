using ToSic.Eav.Caching;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Metadata;

/// <summary>
/// Marks metadata providers.
/// This is important for things that need a source for their metadata, but won't load it till later. 
/// </summary>
[PrivateApi("2020-12-09 v11.11 changed from [InternalApi_DoNotUse_MayChangeWithoutNotice(this is just fyi)]")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IHasMetadataSource: ICacheExpiring
{
    /// <summary>
    /// The metadata source
    /// </summary>
    IMetadataSource MetadataSource { get; }
}