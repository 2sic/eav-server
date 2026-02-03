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
    /// Get the Metadata of the underlying Entity
    /// </summary>
    /// <remarks>
    /// Added in v12.10
    /// </remarks>
    [JsonIgnore]
    IMetadata Metadata { get; }
}