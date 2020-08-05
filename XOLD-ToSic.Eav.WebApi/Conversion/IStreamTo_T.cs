using System.Collections.Generic;
using ToSic.Eav.DataSources;
using ToSic.Eav.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Conversion
{
    /// <summary>
    /// Marks objects which can convert entire DataStreams or DataSources to another format. <br/>
    /// Usually used in serialization scenarios.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public interface IStreamsTo<T>
    {
        /// <summary>
        /// Returns an converted IDataSource, but is serializable.
        /// </summary>
        /// <param name="source">the source</param>
        /// <param name="streams">names of streams to publish. if null, will return all streams</param>
        Dictionary<string, IEnumerable<T>> Convert(IDataSource source, IEnumerable<string> streams = null);

        /// <summary>
        /// Returns an converted IDataSource, but is serializable.
        /// </summary>
        /// <param name="source">the source</param>
        /// <param name="streams">names of streams to publish. if null, will return all streams</param>
        Dictionary<string, IEnumerable<T>> Convert(IDataSource source, string streams);

        /// <summary>
        /// Returns an converted IDataStream, but is serializable.
        /// </summary>
        /// <param name="stream">the source</param>
        IEnumerable<T> Convert(IDataStream stream);
    }
}
