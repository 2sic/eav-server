using System;
using System.Collections;
using System.Collections.Generic;
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
            if (Connection == null) 
                return CreateErrorStream("Missing Connection", "ConnectionStream can't LoadStream()");
            // throw new Exception("Missing connection, can't LoadStream()");

            var source = Connection.DataSource;
            var name = Connection.SourceStream;
            IDataStream stream;
            var noSource = source == null;
            var noName = string.IsNullOrEmpty(name);
            if (noSource || noName)
            {
                stream = Connection.DirectlyAttachedStream;
                if (stream == null)
                    return CreateErrorStream("Missing Source or Name", 
                        $"LoadStream(): No Stream and name or source were also missing - name: '{name}', source: '{source}'");
                //throw new Exception($"LoadStream(): No Stream and name or source were also missing - name: '{name}', source: '{source}'");
            }
            else
            {
                if (!source.Out.ContainsKey(name))
                    return CreateErrorStream("Source doesn't have Stream", $"LoadStream(): Source doesn't have stream '{name}'");
                    //throw new Exception($"LoadStream(): Source doesn't have stream '{name}'");
                stream = source.Out[name];
                if (stream == null)
                    return CreateErrorStream("Source Stream is Null", $"Found stream '{name}' but it's null");
                    //throw new Exception($"Found stream '{name}' but it's null");
            }

            return stream;
        }

        private IDataStream CreateErrorStream(string title, string message)
        {
            var entityList = DataSourceErrorHandling.CreateErrorList(title: title, message: message);
            return new DataStream(null, "ConnectionStreamError", () => entityList);
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

        public IDataSource Source => UnwrappedContents.Source;

        public string Name => UnwrappedContents.Name;

        public DataStreamCacheStatus Caching => UnwrappedContents.Caching;
    }
}
