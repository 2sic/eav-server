using System.Collections.Generic;
using System.Collections.Immutable;
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
        public IEnumerable<IEntity> List => Out[Constants.DefaultStreamName].Immutable;

        [PrivateApi]
        public IImmutableList<IEntity> Immutable => Out[Constants.DefaultStreamName].Immutable;

        #region various Attach-In commands
        /// <inheritdoc />
        [PublicApi]
        public void Attach(IDataSource dataSource)
        {
            // ensure list is blank, otherwise we'll have name conflicts when replacing a source
            Connections.ClearIn();

            foreach (var dataStream in dataSource.Out)
            {
                Attach(dataStream.Key, dataSource, dataStream.Key);
                //AddReplaceIn(dataStream.Key, dataStream.Value);
                //Connections.AddIn(new Connection(dataSource, dataStream.Key, this, dataStream.Key));
            }
        }


        /// <inheritdoc />
        [PublicApi]
        public void Attach(string streamName, IDataSource dataSource, string sourceName = Constants.DefaultStreamName)
        {
            AddReplaceIn(streamName, dataSource[sourceName]);
            
            Connections.AddIn(new Connection(dataSource, sourceName, this, streamName));
        }

        /// <inheritdoc />
        [PublicApi]
        public void Attach(string streamName, IDataStream dataStream)
        {
            AddReplaceIn(streamName, dataStream);
            
            Connections.AddIn(new Connection(dataStream, this, streamName));
        }

        private void AddReplaceIn(string streamName, IDataStream dataStream)
        {
            if (In.ContainsKey(streamName)) In.Remove(streamName);
            In.Add(streamName, dataStream);
        }
        

        #endregion

        #region OutIsDynamic


        /// <inheritdoc />
        [PrivateApi]
        public bool OutIsDynamic { get; protected set; } = false;

        #endregion

    }
}
