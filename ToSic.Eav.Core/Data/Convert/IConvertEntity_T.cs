using ToSic.Eav.Serialization;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

/// <summary>
/// Convert an entity into another format
/// </summary>
/// <typeparam name="T">The target format we'll convert into</typeparam>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface IConvertEntity<out T>: IConvert<IEntity, T>, IConvert<IEntityWrapper, T>, IConvert<object, T>
{

    /// <summary>
    /// Maximum items on a stream to return.
    /// This is primarily used when developing visual query, to limit what is actually sent back to the client.
    /// </summary>
    /// <remarks>
    /// Added v11.13
    /// </remarks>
    [PrivateApi("Exact name / functionality not final yet")]
    int MaxItems { get; set; }

    /// <summary>
    /// Include the entity Guid in the conversion
    /// </summary>
    [PrivateApi("Exact name / functionality not final yet, but this may have leaked to public use, not sure")]
    bool WithGuid { get; set; }

    /// <summary>
    /// Include publishing information (draft etc.) in the conversion
    /// </summary>
    [PrivateApi("Exact name / functionality not final yet, but this may have leaked to public use, not sure")]
    bool WithPublishing { get; }

    /// <summary>
    /// Settings to configure the For-serialization
    /// </summary>
    /// <remarks>
    /// Added in 12.05, was never public
    /// </remarks>
    [PrivateApi]
    MetadataForSerialization MetadataFor { get; }

    /// <summary>
    /// Settings to configure the Metadata-serialization
    /// </summary>
    /// <remarks>
    /// Added in 12.05, was never public
    /// </remarks>
    [PrivateApi]
    ISubEntitySerialization Metadata { get; }

    /// <summary>
    /// Languages to prefer when looking up the values
    /// </summary>
    string[] Languages { get; set; }

    /// <summary>
    /// Ensure all settings are so it includes guids etc.
    /// This is so the serializable information is useful for admin UIs
    /// </summary>
    [PrivateApi("Internal use only")]
    void ConfigureForAdminUse();
        

    [PrivateApi("WIP")]
    TypeSerialization Type { get; set; }
}