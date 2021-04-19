using System;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Represents a data source that can be the recipient of Data.
	/// This basically means it has an In <see cref="IDataStream"/>
	/// </summary>
	[PublicApi_Stable_ForUseInYourCode]
	public interface IDataTarget: IDataPartShared
    {
        /// <summary>
        /// Internal ID usually from persisted configurations IF the configuration was build from an pre-stored query.
        /// </summary>
        /// <returns>The guid of this data source which identifies the configuration <see cref="IEntity"/> of the data source.</returns>
        Guid Guid { get; set; }

		/// <summary>
		/// List of all In connections
		/// </summary>
		IDictionary<string, IDataStream> In { get; }
		
		/// <summary>
		/// Attach a DataSource to In - replaces all existing in-streams.
		/// </summary>
		/// <param name="dataSource">DataSource to attach</param>
		void Attach(IDataSource dataSource);

        /// <summary>
        /// Add a single named stream to the In
        /// </summary>
        /// <param name="streamName">In-name of the stream</param>
        /// <param name="dataSource">The data source - will use it's default out</param>
        /// <param name="sourceName">The stream name on the source, will default to "Default"</param>
        void Attach(string streamName, IDataSource dataSource, string sourceName = Constants.DefaultStreamName);

        /// <summary>
        /// Add a single named stream to the In
        /// </summary>
        /// <param name="streamName">In-name of the stream</param>
        /// <param name="dataStream">The data stream to attach</param>
        void Attach(string streamName, IDataStream dataStream);
	}
}
