using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
    public abstract partial class DataSource
    {
        #region Connections

        internal Connections Connections => _connections ?? (_connections = new Connections(this));
        private Connections _connections;

        #endregion

        /// <inheritdoc />
        [PublicApi]
        public IDictionary<string, IDataStream> In { get; internal set; } = new Dictionary<string, IDataStream>();

        /// <inheritdoc />
        [PublicApi]
        public IImmutableList<IEntity> TryGetIn(string name = Constants.DefaultStreamName) => !In.ContainsKey(name) ? null : In[name]?.List?.ToImmutableList();

        /// <inheritdoc />
        [PublicApi]
        public virtual IDictionary<string, IDataStream> Out { get; protected internal set; } = new StreamDictionary();

        /// <inheritdoc />
        public IDataStream this[string outName] => GetStream(outName);

        /// <inheritdoc />
        [PublicApi]
        public IDataStream GetStream(string name = null, string noParamOrder = Parameters.Protector, bool nullIfNotFound = false, bool emptyIfNotFound = false)
        {
            Parameters.ProtectAgainstMissingParameterNames(noParamOrder, nameof(GetStream), $"{nameof(nullIfNotFound)}");

            // Check if streamName was not provided
            if (string.IsNullOrEmpty(name)) name = Constants.DefaultStreamName;

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


        /// <inheritdoc />
        [PublicApi]
        public void Attach(string streamName, IDataSource dataSource, string sourceName = Constants.DefaultStreamName)
        {
            var connection = new Connection(dataSource, sourceName, this, streamName);
            ConnectIn(connection);
        }

        /// <inheritdoc />
        [PublicApi]
        public void Attach(string streamName, IDataStream dataStream)
        {
            var connection = new Connection(dataStream, this, streamName);
            ConnectIn(connection);
        }
        
        private void ConnectIn(Connection connection)
        {
            var connStream = new ConnectionStream(connection, Error);
            if (In.ContainsKey(connection.TargetStream)) In.Remove(connection.TargetStream);
            In.Add(connection.TargetStream, connStream);
            Connections.AddIn(connection);
        }
        

        #endregion
    }
}
