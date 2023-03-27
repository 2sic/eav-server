﻿using System.Collections;
using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.DataSource;
using ToSic.Lib.Data;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.DataSources
{
    [PrivateApi]
    public class ConnectionStream: IDataStream, IWrapper<IDataStream>
    {

        public ConnectionStream(DataSourceConnection connection, DataSourceErrorHelper errorHandler = null)
        {
            Connection = connection;
            _errorHandler = errorHandler;
        }

        public DataSourceConnection Connection;
        private readonly DataSourceErrorHelper _errorHandler;

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
            var errors = _errorHandler.Create(title: title, message: message);
            return new DataStream(intendedSource, "ConnectionStreamError", () => errors);
        }

        public IDataStream GetContents() => UnwrappedDataStream;
        private IDataStream UnwrappedDataStream => _dataStream.Get(LoadStream);
        private readonly GetOnce<IDataStream> _dataStream = new GetOnce<IDataStream>();


        #region Simple properties linked to the underlying Stream

        public bool AutoCaching
        {
            get => UnwrappedDataStream.AutoCaching;
            set => UnwrappedDataStream.AutoCaching = value;
        }

        public int CacheDurationInSeconds
        {
            get => UnwrappedDataStream.CacheDurationInSeconds;
            set => UnwrappedDataStream.CacheDurationInSeconds = value;
        }

        public bool CacheRefreshOnSourceRefresh
        {
            get => UnwrappedDataStream.CacheRefreshOnSourceRefresh;
            set => UnwrappedDataStream.CacheRefreshOnSourceRefresh = value;
        }

        public void PurgeList(bool cascade = false) => UnwrappedDataStream.PurgeList(cascade);

        public IEnumerator<IEntity> GetEnumerator() => UnwrappedDataStream.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => UnwrappedDataStream.GetEnumerator();

        public IEnumerable<IEntity> List => UnwrappedDataStream.List;

        public IDataSource Source => UnwrappedDataStream.Source;

        public string Name => UnwrappedDataStream.Name;
        public string Scope => UnwrappedDataStream.Scope;

        public DataStreamCacheStatus Caching => UnwrappedDataStream.Caching;

        #endregion
    }
}