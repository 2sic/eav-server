using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.DataSources
{
    [PrivateApi("Experimental")]
    public class ConnectionStream: IDataStream, IWrapper<IDataStream>
    {
        public ConnectionStream(Connection connection) => Connection = connection;

        public Connection Connection;

        private IDataStream LoadStream()
        {
            if (Connection == null) throw new Exception("Missing connection, can't LoadStream()");

            var source = Connection.DataSource;
            var name = Connection.SourceStream;
            IDataStream stream;
            var noSource = source == null;
            var noName = string.IsNullOrEmpty(name);
            if (noSource || noName)
            {
                stream = Connection.DirectlyAttachedStream;
                if (stream == null)
                    throw new Exception($"LoadStream(): No Stream and name or source were also missing - name: '{name}', source: '{source}'");
            }
            else
            {
                if (!source.Out.ContainsKey(name)) throw new Exception($"LoadStream(): Source doesn't have stream '{name}'");
                stream = source.Out[name];
                if (stream == null) throw new Exception($"Found stream '{name}' but it's null");
            }

            return stream;
        }

        public IDataStream UnwrappedContents => _dataStream ?? (_dataStream = LoadStream());

        private IDataStream _dataStream;

        public bool AutoCaching
        {
            get => UnwrappedContents.AutoCaching;
            set => UnwrappedContents.AutoCaching = value;
        }

        public int CacheDurationInSeconds
        {
            get => UnwrappedContents.CacheDurationInSeconds;
            set => UnwrappedContents.CacheDurationInSeconds = value;
        }

        public bool CacheRefreshOnSourceRefresh
        {
            get => UnwrappedContents.CacheRefreshOnSourceRefresh;
            set => UnwrappedContents.CacheRefreshOnSourceRefresh = value;
        }

        public void PurgeList(bool cascade = false) => UnwrappedContents.PurgeList(cascade);

        public IEnumerator<IEntity> GetEnumerator() => UnwrappedContents.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => UnwrappedContents.GetEnumerator();

        public IEnumerable<IEntity> List => UnwrappedContents.List;

        public IImmutableList<IEntity> Immutable => UnwrappedContents.Immutable;

        public IDataSource Source => UnwrappedContents.Source;

        public string Name => UnwrappedContents.Name;

        public DataStreamCacheStatus Caching => UnwrappedContents.Caching;
    }
}
