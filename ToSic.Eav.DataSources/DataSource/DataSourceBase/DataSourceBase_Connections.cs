using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using ToSic.Eav.Data;
using ToSic.Eav.DataSource.Streams;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using static System.StringComparer;

namespace ToSic.Eav.DataSource
{
    public abstract partial class DataSourceBase
    {
        #region Connections

        [InternalApi_DoNotUse_MayChangeWithoutNotice]
        internal DataSourceConnections Connections => _connections ?? (_connections = new DataSourceConnections(this));
        private DataSourceConnections _connections;

        #endregion

        /// <inheritdoc />
        [PublicApi]
        public virtual IReadOnlyDictionary<string, IDataStream> In => _in.Get(() => new ReadOnlyDictionary<string, IDataStream>(_inRw));
        private GetOnce<IReadOnlyDictionary<string, IDataStream>> _in = new GetOnce<IReadOnlyDictionary<string, IDataStream>>();
        private IDictionary<string, IDataStream> _inRw = new Dictionary<string, IDataStream>(InvariantCultureIgnoreCase);

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
        /// Introduced in 2sxc 15.04
        /// </remarks>
        [PublicApi]
        protected internal IImmutableList<IEntity> TryGetIn(string name = DataSourceConstants.StreamDefaultName) => !In.ContainsKey(name) ? null : In[name]?.List?.ToImmutableList();

        /// <inheritdoc />
        [PublicApi]
        public virtual IReadOnlyDictionary<string, IDataStream> Out => _outRw.AsReadOnly(); // { get; protected internal set; } = new StreamDictionary();
        private StreamDictionary _outRw = new StreamDictionary();

        /// <inheritdoc />
        public IDataStream this[string outName] => GetStream(outName);

        /// <inheritdoc />
        [PublicApi]
        public IDataStream GetStream(string name = null, string noParamOrder = Parameters.Protector, bool nullIfNotFound = false, bool emptyIfNotFound = false)
        {
            Parameters.ProtectAgainstMissingParameterNames(noParamOrder, nameof(GetStream), $"{nameof(nullIfNotFound)}");

            // Check if streamName was not provided
            if (string.IsNullOrEmpty(name)) name = DataSourceConstants.StreamDefaultName;

            // Simple case - just get it
            if (Out.ContainsKey(name)) return Out[name];

            if (nullIfNotFound && emptyIfNotFound)
                throw new ArgumentException($"You cannot set both {nameof(nullIfNotFound)} and {nameof(emptyIfNotFound)} to true");

            // If null is preferred to an error, return this
            if (nullIfNotFound) return null;

            // If empty is preferred to an error, return this
            if (emptyIfNotFound) return new DataStream(this, name, () => new List<IEntity>());

            // Not found and no rule to handle it, throw error
            throw new KeyNotFoundException(
                $"Can't find Stream with the name '{name}'. This could be a typo. Otherwise we recommend that you use either " +
                $"'{nameof(nullIfNotFound)}: true' (for null-checks or ?? chaining) " +
                $"or '{nameof(emptyIfNotFound)}: true' (for situations where you just want to add LINQ statements"
            );
        }

        /// <inheritdoc />
        [PublicApi]
        public IEnumerable<IEntity> List => GetStream().List;


        #region various Attach-In commands
        /// <inheritdoc />
        [PublicApi]
        public void Attach(IDataSource dataSource)
        {
            foreach (var dataStream in dataSource.Out) 
                Attach(dataStream.Key, dataSource, dataStream.Key);
        }

        [PrivateApi]
        private void Connect(IDataSourceLink connections)
        {
            var list = connections.Flatten();
            foreach (var link in list)
                if (link.Stream != null)
                    Attach(link.InName, link.Stream);
                else
                    Attach(link.InName, link.DataSource, link.OutName);
        }



        /// <inheritdoc />
        [PublicApi]
        public void Attach(string streamName, IDataSource dataSource, string sourceName = DataSourceConstants.StreamDefaultName) 
            => Attach(new DataSourceConnection(dataSource, sourceName, this, streamName));

        /// <inheritdoc />
        [PublicApi]
        public void Attach(string streamName, IDataStream dataStream)
        {
            if (dataStream == null) return;
            Attach(new DataSourceConnection(dataStream, this, streamName));
        }

        private void Attach(DataSourceConnection connection)
        {
            if (Immutable) throw new Exception($"This data source is Immutable. Attaching more sources after creation is not allowed. DataSource: {GetType().Name}");
            _inRw[connection.TargetStream] = new ConnectionStream(connection, Error);
            Connections.AddIn(connection);
        }
        

        #endregion
    }
}
