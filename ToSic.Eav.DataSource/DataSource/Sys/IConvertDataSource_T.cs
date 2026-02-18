using ToSic.Eav.DataFormats.EavLight;

namespace ToSic.Eav.DataSource.Sys;

/// <summary>
/// Marks objects which can convert a DataSource to another format.
///
/// This will always return some kind of dictionary with stream-names and the converted items as sub-lists. 
/// Usually used in serialization scenarios.
/// </summary>
/// <typeparam name="T"></typeparam>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IConvertDataSource<T>
{
    /// <summary>
    /// Returns a converted IDataSource, but is serializable.
    /// </summary>
    /// <param name="source">the source</param>
    /// <param name="streams">names of streams to publish. if null, will return all streams</param>
    IDictionary<string, IEnumerable<T>> Convert(IDataSource source, IEnumerable<string>? streams = null);

    /// <summary>
    /// Returns a converted IDataSource, but is serializable.
    /// </summary>
    /// <param name="source">the source</param>
    /// <param name="streams">names of streams to publish. if null, will return all streams</param>
    IDictionary<string, IEnumerable<T>> Convert(IDataSource source, string streams);


    [PrivateApi("not public yet, as the signature is not final yet")]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    IDictionary<string, IEnumerable<T>> Convert(IDataSource source, IEnumerable<string> streams, string[] filterGuids);


    /// <summary>
    /// WIP v21
    /// </summary>
    /// <param name="source"></param>
    /// <param name="streams"></param>
    /// <param name="filterGuids"></param>
    /// <param name="selectFields"></param>
    /// <returns></returns>
    public IDictionary<string, IEnumerable<EavLightEntity>> Convert(
        IDataSource source,
        IEnumerable<string>? streams,
        string[]? filterGuids,
        IDictionary<string, ICollection<string>>? selectFields
    );

}