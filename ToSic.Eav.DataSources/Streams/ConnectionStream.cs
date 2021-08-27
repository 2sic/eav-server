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

            var ds = Connection.DataSource;
            var name = Connection.SourceStream;
            IDataStream stream;
            var noSource = ds == null;
            var noName = string.IsNullOrEmpty(name);
            if (noSource || noName)
            {
                stream = Connection.DirectlyAttachedStream;
                if (stream == null)
                    return CreateErrorStream("Missing Source or Name", 
                        $"LoadStream(): No Stream and name or source were also missing - name: '{name}', source: '{ds}'");
            }
            else
            {
                if (!ds.Out.ContainsKey(name))
                    return CreateErrorStream("Source doesn't have Stream", $"LoadStream(): Source '{ds.Label}' doesn't have stream '{name}'", ds);
                stream = ds.Out[name];
                if (stream == null)
                    return CreateErrorStream("Source Stream is Null", $"Source '{ds.Label}' has stream '{name}' but it's null", ds);
            }

            return stream;
        }

        private IDataStream CreateErrorStream(string title, string message, IDataSource intendedSource = null)
        {
            var errorHandler = Connection.DataSource?.ErrorHandler
                               ?? Factory.Resolve<DataSourceErrorHandling>();
            var entityList = errorHandler.CreateErrorList(title: title, message: message);
            return new DataStream(intendedSource, "ConnectionStreamError", () => entityList);
        }

        public IDataStream UnwrappedContents => _dataStream ?? (_dataStream = LoadStream());

        private IDataStream _dataStream;

        #region Simple properties linked to the underlying Stream

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

        #endregion
    }
}
