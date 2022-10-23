using System.Collections.Generic;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    /// <summary>
    /// Marks objects which can convert a DataSource to another format.
    ///
    /// This will always return some kind of dictionary with stream-names and the converted items as sub-lists. 
    /// Usually used in serialization scenarios.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public interface IConvertDataSource<T>
    {
        /// <summary>
        /// Returns an converted IDataSource, but is serializable.
        /// </summary>
        /// <param name="source">the source</param>
        /// <param name="streams">names of streams to publish. if null, will return all streams</param>
        IDictionary<string, IEnumerable<T>> Convert(IDataSource source, IEnumerable<string> streams = null);

        /// <summary>
        /// Returns an converted IDataSource, but is serializable.
        /// </summary>
        /// <param name="source">the source</param>
        /// <param name="streams">names of streams to publish. if null, will return all streams</param>
        IDictionary<string, IEnumerable<T>> Convert(IDataSource source, string streams);


        [PrivateApi("not public yet, as the signature is not final yet")]
        IDictionary<string, IEnumerable<T>> Convert(IDataSource source, IEnumerable<string> streams, string[] filterGuids);

    }
}
