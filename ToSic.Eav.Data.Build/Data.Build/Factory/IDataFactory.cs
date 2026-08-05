using System.Collections.Immutable;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;
using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Data.Build;


/// <summary>
/// A data builder which will generate items for a specific type.
/// In many cases it will also take care of auto increasing the id and more.
/// </summary>
/// <remarks>
/// * Added in v15 to replace the previous IDataBuilder
/// * v22 changed some internals to be more flexible, assume not intensively used so no issues expected
/// </remarks>
[PublicApi]
public interface IDataFactory: IServiceRespawn<IDataFactory, DataFactoryOptions>, IServiceWithSetup<DataFactoryOptions>
{
    /// <summary>
    /// A counter for the ID in case the data provided doesn't have an ID to use.
    /// Default is `1`.
    /// Negative numbers are possible.
    /// </summary>
    int IdCounter { get; }


    /// <summary>
    /// The generated ContentType.
    /// This will only be prepared once, for better performance.
    /// </summary>
    /// <remarks>
    /// * Set to internal v22 as the first access could change what happens,
    ///   and should not be done before the first IRawEntity conversion.
    /// * Will prioritize according to the internal logic of the <see cref="DataFactoryContentTypeHelper"/>
    /// </remarks>
    internal IContentType? ContentType { get; }

    /// <summary>
    /// TODO:
    /// </summary>
    [PrivateApi]
    ILookup<object, IEntity> Relationships { get; }

    #region Simple Create

    /// <summary>
    /// Create a single entity based on values passed in.
    /// </summary>
    /// <returns></returns>
    IEntity Create(
        IDictionary<string, object?> values,
        int id = default,
        Guid guid = default,
        DateTime created = default,
        DateTime modified = default,
        // experimental v18.02
        EntityPartsLazy? partsLazy = default);

    /// <summary>
    /// Create an entity from a single <see cref="IRawEntity"/>
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    IEntity Create(IRawData item);


    #endregion


    #region Create List

    /// <summary>
    /// Create a complete list of <see cref="IRawEntity"/>s.
    /// This is the method to use when you don't plan on doing any post-processing.
    ///
    /// If you need post-processing, call `Prepare` instead and finish using `WrapUp`.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    IImmutableList<IEntity> Create<T>(IEnumerable<T> list) where T : class, IRawData;

    #endregion
    
}