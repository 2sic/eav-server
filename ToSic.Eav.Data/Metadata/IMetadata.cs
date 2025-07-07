using ToSic.Sys.Caching;
using ToSic.Sys.Security.Permissions;


namespace ToSic.Eav.Metadata;

/// <summary>
/// A provider for metadata for something.
/// So if an <see cref="IEntity"/> or an App has metadata, this will provide it. 
/// </summary>
/// <remarks>
/// You can either loop through this object (since it's an `IEnumerable`) or ask for values of the metadata,
/// no matter on what sub-entity the value is stored on.
/// </remarks>
[PublicApi]
public interface IMetadata: IEnumerable<IEntity>, IHasPermissions, ITimestamped
{
    /// <summary>
    /// Get the best matching value in ALL the metadata items.
    /// </summary>
    /// <typeparam name="TVal">expected type, like string, int etc.</typeparam>
    /// <param name="name">attribute name we're looking for</param>
    /// <returns>A typed value. </returns>
    /// <remarks>
    /// In most scenarios, Metadata only has one additional item, so this will be accessed when using the simple Get.
    /// If you expect objects with many metadata items which risk containing the same fields - such as `Title`,
    /// best use the more specific Get with a type-name overload to specify the type-name.
    /// </remarks>
    TVal? Get<TVal>(string name);

    /// <summary>
    /// Get the best matching value in the metadata items.
    /// </summary>
    /// <typeparam name="TVal">expected type, like string, int etc.</typeparam>
    /// <param name="name">attribute name we're looking for</param>
    /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="typeName">Optional single type-name. If provided, will only look at metadata of that type; otherwise (or if null) will look at all metadata items and pick first match</param>
    /// <param name="typeNames">
    /// List of type-name in the order it will check.
    /// If one of the values in the list is `null`, it will then check all items no matter what type.
    /// So it's common to specify preferred types first, and then a `null` at the end to catch all other types.
    /// </param>
    /// <remarks>
    /// Depending on the provided type-parameters, it will only look in certain items, or all items.
    ///
    /// History
    ///
    /// * Name changed from old `GetBestValue` to simple `Get` in v20.
    /// </remarks>
    /// <returns>A typed value. </returns>
    // ReSharper disable once MethodOverloadWithOptionalParameter
    TVal? Get<TVal>(string name, NoParamOrder noParamOrder = default, string? typeName = null, IEnumerable<string?>? typeNames = default);


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
    /// Get all Metadata entities of a specific type
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