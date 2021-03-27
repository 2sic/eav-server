using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    public abstract partial class DataSourceBase
    {
        #region Experimental new Connections

        internal Connections Connections => _connections ?? (_connections = new Connections(this));
        private Connections _connections;
        #endregion

        /// <inheritdoc />
        public IDictionary<string, IDataStream> In { get; internal set; } = new Dictionary<string, IDataStream>();

        /// <inheritdoc />
        public virtual IDictionary<string, IDataStream> Out { get; protected internal set; } = new StreamDictionary();

        /// <inheritdoc />
        public IDataStream this[string outName] => Out[outName];

        /// <inheritdoc />
        public IEnumerable<IEntity> List => Out[Constants.DefaultStreamName].List;


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
            var connStream = new ConnectionStream(connection);
            if (In.ContainsKey(connection.TargetStream)) In.Remove(connection.TargetStream);
            In.Add(connection.TargetStream, connStream);
            Connections.AddIn(connection);
        }
        

        #endregion

        #region OutIsDynamic


        ///// <inheritdoc />
        //[PrivateApi]
        //public bool OutIsDynamic { get; protected set; } = false;

        #endregion

    }
}
