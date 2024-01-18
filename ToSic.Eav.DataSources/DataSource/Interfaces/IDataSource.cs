using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.DataSource.Internal.Caching;
using ToSic.Eav.DataSources;
using ToSic.Lib.Coding;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSource;

/// <summary>
/// Public interface for an Eav DataSource. All DataSource objects are based on this. 
/// </summary>
[PublicApi]
public interface IDataSource : IDataSourceLinkable, IAppIdentity, ICacheInfo, IHasLog
#pragma warning disable CS0618
    , IDataTarget
#pragma warning restore CS0618
{
    #region MyRegion

    /// <summary>
    /// Internal ID usually from persisted configurations IF the configuration was build from an pre-stored query.
    /// </summary>
    /// <returns>The guid of this data source which identifies the configuration <see cref="IEntity"/> of the data source.</returns>
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    Guid Guid { get; }

    /// <summary>
    /// Name of this DataSource - not usually relevant.
    /// </summary>
    /// <returns>Name of this source.</returns>
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    string Name { get; }

    /// <summary>
    /// internal use only - for labeling data sources in queries to show in debugging
    /// </summary>
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    string Label { get; }

    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    void AddDebugInfo(Guid? guid, string label);

    #endregion

    #region Data Interfaces

    /// <summary>
    /// Gets the Dictionary of Out-Streams. This is the internal accessor, as usually you'll use this["name"] instead. <br/>
    /// In rare cases you need the Out, for example to list the stream names in the data source.
    /// </summary>
    /// <returns>A dictionary of named <see cref="IDataStream"/> objects, case insensitive</returns>
    IReadOnlyDictionary<string, IDataStream> Out { get; }

    /// <summary>
    /// Gets the Out-Stream with specified Name. 
    /// </summary>
    /// <returns>an <see cref="IDataStream"/> of the desired name</returns>
    /// <exception cref="NullReferenceException">if the stream does not exist</exception>
    IDataStream this[string outName] { get; }

    /// <summary>
    /// Gets the Out-Stream with specified Name and allowing some error handling if not found.
    /// </summary>
    /// <param name="name">The desired stream name. If empty, will default to the default stream.</param>
    /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="nullIfNotFound">In case the stream `name` isn't found, will return null. Ideal for chaining with ??</param>
    /// <param name="emptyIfNotFound">In case the stream `name` isn't found, will return an empty stream. Ideal for using LINQ directly.</param>
    /// <returns>an <see cref="IDataStream"/> of the desired name</returns>
    /// <exception cref="NullReferenceException">if the stream does not exist and `nullIfNotFound` is false</exception>
    /// <remarks>
    /// 1. Added in 2sxc 12.05
    /// 1. for more in-depth checking if a stream exists, you can access the <see cref="Out"/> which is an IDictionary
    /// </remarks>
    IDataStream GetStream(string name = null, NoParamOrder noParamOrder = default, bool nullIfNotFound = false, bool emptyIfNotFound = false);

    /// <summary>
    /// The items in the data-source - to be exact, the ones in the Default stream.
    /// </summary>
    /// <returns>A list of <see cref="IEntity"/> items in the Default stream.</returns>
    IEnumerable<IEntity> List { get; }

    /// <summary>
    /// List of all In connections.
    /// </summary>
    /// <returns>A dictionary of named <see cref="IDataStream"/> objects, case insensitive</returns>
    IReadOnlyDictionary<string, IDataStream> In { get; }

    #endregion
    #region Config

    /// <summary>
    /// The configuration system of this data source.
    /// Keeps track of all values which the data source will need, and manages the LookUp engine
    /// which provides these values. 
    /// </summary>
    IDataSourceConfiguration Configuration { get; }

    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    void Setup(IDataSourceOptions options, IDataSourceLinkable attach);

    #endregion

    #region Caching Information

    /// <summary>
    /// Some configuration of the data source is cache-relevant, others are not.
    /// This list contains the names of all configuration items which are cache relevant.
    /// It will be used when generating a unique ID for caching the data.
    /// </summary>
    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    List<string> CacheRelevantConfigurations { get; }

    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    ICacheKeyManager CacheKey { get; }

    #endregion

    #region Error Handler

    /// <summary>
    /// Special helper to generate error-streams.
    ///
    /// DataSources should never `throw` exceptions but instead return a stream containing the error information.
    /// </summary>
    DataSourceErrorHelper Error { get; }


    #endregion

    /// <summary>
    /// Information if the DataSource is Immutable.
    /// Reason is that starting in v15, everything should become immutable.
    /// So setting parameters or attaching other sources will not be possible any more after initial creation.
    /// But because a lot of code is still out there which assumes mutable objects, this is set depending on how the DataSource was created.
    /// Newer APIs will result in Immutable DataSources, while older APIs will get you a mutable DataSource.
    /// See [](xref:NetCode.Conventions.Immutable).
    /// </summary>
    /// <remarks>
    /// New in 15.06
    /// </remarks>
    bool Immutable { get; }

    [PrivateApi]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    void DoWhileOverrideImmutable(Action action);
}