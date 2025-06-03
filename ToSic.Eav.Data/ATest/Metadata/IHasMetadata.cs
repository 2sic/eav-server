using System.Text.Json.Serialization;

namespace ToSic.Eav.Metadata;

/// <summary>
/// Anything with this interface has a property `Metadata` which can give us more
/// information about that object. 
/// </summary>
[PublicApi]
public interface IHasMetadata
{
    /// <summary>
    /// Additional information, specs etc. about this thing which has metadata
    /// </summary>
    [JsonIgnore]    // 2024-07-02 experimental, usually explicitly implemented and not meant for serialization
    IMetadataOf Metadata { get; }
}