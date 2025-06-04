using ToSic.Sys.Caching;
using ToSic.Sys.Security.Permissions;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Metadata;

/// <summary>
/// A provider for metadata for something.
/// So if an <see cref="IEntity"/> or an App has metadata, this will provide it. 
/// </summary>
/// <remarks>
/// You can either loop through this object (since it's an `IEnumerable`) or ask for values of the metadata,
/// no matter on what sub-entity the value is stored on.</remarks>
[PublicApi]
public interface IMetadataOf: IEnumerable<IEntity>, IHasPermissions, ITimestamped
{

    /// <summary>
    /// Get the best matching value in ALL the metadata items.
    /// </summary>
    /// <typeparam name="TVal">expected type, like string, int etc.</typeparam>
    /// <param name="name">attribute name we're looking for</param>
    /// <param name="typeName">optional type-name, if provided, will only look at metadata of that type; otherwise (or if null) will look at all metadata items and pick first match</param>
    /// <returns>A typed value. </returns>
    TVal GetBestValue<TVal>(string name, string typeName = null);


    /// <summary>
    /// Get the best matching value in the metadata items.
    /// </summary>
    /// <typeparam name="TVal">expected type, like string, int etc.</typeparam>
    /// <param name="name">attribute name we're looking for</param>
    /// <param name="typeNames">list of type-name in the order to check. if one of the values is null, it will then check all items no matter what type</param>
    /// <returns>A typed value. </returns>
    TVal GetBestValue<TVal>(string name, string[] typeNames);

    /// <summary>
    /// Determine if something has metadata of the specified type
    /// </summary>
    /// <param name="typeName">Type Name</param>
    /// <returns>True if there is at least one item of this type</returns>
    /// <remarks>
    /// Added in v13
    /// </remarks>
    bool HasType(string typeName);

    /// <summary>
    /// Get all Metadata items of a specific type
    /// </summary>
    /// <param name="typeName">Type Name</param>
    /// <returns></returns>
    /// <remarks>
    /// Added in v13
    /// </remarks>
    IEnumerable<IEntity> OfType(string typeName);

    /// <summary>
    /// The identifier which was used to retrieve the Metadata.
    /// It can be used as an address for creating further Metadata for the same target. 
    /// </summary>
    /// <remarks>
    /// Added in v13
    /// </remarks>
    ITarget Target { get; }
}