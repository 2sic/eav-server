using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Linking;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSource
{
    /// <summary>
    /// Represents a data source that can be the recipient of Data.
    /// This basically means it has an In <see cref="IDataStream"/>
    /// </summary>
    /// <remarks>
    /// New in v15.04 as it replaces the old `IDataTarget` because of confusing names.
    /// </remarks>
    [PublicApi]
	public interface IDataSourceTarget: IDataSourceShared, 
#pragma warning disable CS0618
        IDataTarget
#pragma warning restore CS0618
    {
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
        void Attach(string streamName, IDataSource dataSource, string sourceName = DataSourceConstants.StreamDefaultName);

        /// <summary>
        /// Add a single named stream to the In
        /// </summary>
        /// <param name="streamName">In-name of the stream</param>
        /// <param name="dataStream">The data stream to attach</param>
        void Attach(string streamName, IDataStream dataStream);

        [PrivateApi]
        void Connect(IDataSourceLink connections);

        /// <summary>
        /// Get a specific Stream from In.
        /// If it doesn't exist return false and place the error message in the list for returning to the caller.
        ///
        /// Usage usually like this in your GetList() function: 
        /// <code>
        /// private IImmutableList&lt;IEntity&gt; GetList()
        /// {
        ///   var source = TryGetIn();
        ///   if (source is null) return Error.TryGetInFailed(this);
        ///   var result = source.Where(s => ...).ToImmutableList();
        ///   return result;
        /// }
        /// </code>
        /// Or if you're using [Call Logging](xref:NetCode.Logging.Index) do something like this:
        /// <code>
        /// private IImmutableList&lt;IEntity&gt; GetList() => Log.Func(l =>
        /// {
        ///   var source = TryGetIn();
        ///   if (source is null) return (Error.TryGetInFailed(this), "error");
        ///   var result = source.Where(s => ...).ToImmutableList();
        ///   return (result, $"ok - found: {result.Count}");
        /// });
        /// </code>
        /// </summary>
        /// <param name="name">Stream name - optional</param>
        /// <returns>A list containing the data, or null if not found / something breaks.</returns>
        /// <remarks>
        /// Introduced in 2sxc 11.13
        /// </remarks>
        [PublicApi]
        IImmutableList<IEntity> TryGetIn(string name = DataSourceConstants.StreamDefaultName);

    }
}
