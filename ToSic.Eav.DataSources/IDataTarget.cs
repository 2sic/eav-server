using System.Collections.Generic;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Represents a data source that can retrieve Data
	/// </summary>
	public interface IDataTarget
	{
		/// <summary>
		/// In Connections
		/// </summary>
		IDictionary<string, IDataStream> In { get; }
		
		/// <summary>
		/// Attach specified DataSource to In - replaces all existing in-streams
		/// </summary>
		/// <param name="dataSource">DataSource to attach</param>
		void Attach(IDataSource dataSource);

        /// <summary>
        /// Attach a single named stream to the In
        /// </summary>
        /// <param name="streamName">In-name of the stream</param>
        /// <param name="dataSource">The data source - will use it's default out</param>
	    void Attach(string streamName, IDataSource dataSource);

        /// <summary>
        /// Attach a single named stream to the In
        /// </summary>
        /// <param name="streamName">In-name of the stream</param>
        /// <param name="dataStream">The data stream to attach</param>
        void Attach(string streamName, IDataStream dataStream);
	}
}
