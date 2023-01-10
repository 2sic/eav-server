﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Caching;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// A DataStream to get Entities when needed
    /// </summary>
    [PrivateApi]
    public class DataStream : IDataStream, IHasLog
    {
        private readonly GetImmutableListDelegate _listDelegate;

        #region Self-Caching and Results-Persistence Properties / Features

        /// <inheritdoc />
        /// <summary>
        /// Place the stream in the cache if wanted, by default not
        /// </summary>
        public bool AutoCaching { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Default cache duration is 1 day = 3600 * 24
        /// </summary>
        public int CacheDurationInSeconds { get; set; } = 3600 * 24; // one day, since by default if it caches, it would check upstream for cache-reload


        /// <inheritdoc />
        /// <summary>
        /// Kill the cache if the source data is newer than the cache-stamped data
        /// </summary>
        public bool CacheRefreshOnSourceRefresh { get; set; } = true;

        /// <inheritdoc />
        public string Scope { get; protected internal set; } = Scopes.Default; //  Constants.ScopeContentFuture;

        /// <summary>
        /// Provide access to the CacheKey - so it could be overridden if necessary without using the stream underneath it
        /// </summary>
        public virtual DataStreamCacheStatus Caching => _cachingInternal ?? (_cachingInternal = new DataStreamCacheStatus(Source, Source, Name));
        private DataStreamCacheStatus _cachingInternal;


        #endregion

        /// <summary>
        /// Constructs a new DataStream
        /// </summary>
        /// <param name="source">The DataSource providing Entities when needed</param>
        /// <param name="name">Name of this Stream</param>
        /// <param name="listDelegate">Function which gets Entities</param>
        /// <param name="enableAutoCaching"></param>
        public DataStream(IDataSource source, string name, GetIEnumerableDelegate listDelegate = null, bool enableAutoCaching = false)
            : this(source, name, ConvertDelegate(listDelegate), enableAutoCaching) { }

        public DataStream(IDataSource source, string name, GetImmutableArrayDelegate listDelegate = null, bool enableAutoCaching = false)
            : this(source, name, ConvertDelegate(listDelegate), enableAutoCaching) { }

        /// <summary>
        /// Constructs a new DataStream
        /// </summary>
        /// <param name="source">The DataSource providing Entities when needed</param>
        /// <param name="name">Name of this Stream</param>
        /// <param name="listDelegate">Function which gets Entities</param>
        /// <param name="enableAutoCaching"></param>
        public DataStream(IDataSource source, string name, GetImmutableListDelegate listDelegate = null, bool enableAutoCaching = false)
        {
            Source = source;
            Name = name;
            _listDelegate = listDelegate;
            AutoCaching = enableAutoCaching;
        }

        private static GetImmutableListDelegate ConvertDelegate(GetIEnumerableDelegate original)
        {
            if (original == null) return null;
            return () =>
            {
                var initialResult = original();
                return initialResult as IImmutableList<IEntity> ?? initialResult.ToImmutableArray();
            };
        }

        private static GetImmutableListDelegate ConvertDelegate(GetImmutableArrayDelegate original)
            => original == null ? (GetImmutableListDelegate)null : () => original();

        #region Get Dictionary and Get List

        /// <summary>
        /// A temporary result list - must be a List, because otherwise
        /// there's a high risk of IEnumerable signatures with functions being stored inside
        /// </summary>
        /// <remarks>
        /// Note that were possible, it will be an ImmutableSmartList wrapping an ImmutableArray for maximum performance.
        /// </remarks>
	    private IImmutableList<IEntity> _list;

        private bool _listLoaded;
        private ILog _log;

        public IEnumerable<IEntity> List
        {
            get
            {
                // Note about Logging
                // In rare cases the Source is null - and we don't want to cause Errors just because we can't log
                // These cases usually occur when error-streams are created - in which case they sometimes don't have a source
                var wrapLog = Log.Fn<IImmutableList<IEntity>>($"{nameof(Name)}:{Name}", timer: true);

                // If already retrieved return last result to be faster
                if (_listLoaded) return wrapLog.Return(_list, "reuse previous");

                // Check if it's in the cache - and if yes, if it's still valid and should be re-used --> return if found
                if (AutoCaching)
                {
                    Log.A($"{nameof(AutoCaching)}:{AutoCaching}");
                    var cacheItem = new ListCache(Log).GetOrBuild(this, ReadUnderlyingList, CacheDurationInSeconds);
                    _list = cacheItem.List;
                }
                else
                    _list = ReadUnderlyingList();

                _listLoaded = true;
                return wrapLog?.ReturnAsOk(_list) ?? _list;
            }
        }


        /// <summary>
        /// Assemble the list - from the initially configured ListDelegate
        /// </summary>
        /// <returns></returns>
        IImmutableList<IEntity> ReadUnderlyingList()
        {
            var wrapLog = Source.Log.Fn<IImmutableList<IEntity>>();
            // try to use the built-in Entities-Delegate, but if not defined, use other delegate; just make sure we test both, to prevent infinite loops
            if (_listDelegate == null)
                return wrapLog.Return(Source.ErrorHandler.CreateErrorList(source: Source,
                    title: "Error loading Stream",
                    message: "Can't load stream - no delegate found to supply it"),
                    "error");

            try
            {
                var resultList = ImmutableSmartList.Wrap(_listDelegate());
                return wrapLog.ReturnAsOk(resultList);
            }
            catch (InvalidOperationException invEx) // this is a special exception - for example when using SQL. Pass it on to enable proper testing
            {
                return wrapLog.Return(Source.ErrorHandler.CreateErrorList(source: Source, title: "InvalidOperationException", message: "See details", exception: invEx),
                    "error");
            }
            catch (Exception ex)
            {
                return wrapLog.Return(Source.ErrorHandler.CreateErrorList(source: Source, exception: ex,
                    title: "Error getting Stream / reading underlying list",
                    message: $"Error getting List of Stream.\nStream Name: {Name}\nDataSource Name: {Source.Name}"),
                    "error");
            }
        }
        #endregion



        public void PurgeList(bool cascade = false)
        {
            var callLog = Log.Fn(message: $"PurgeList on Stream: {Name}, {nameof(cascade)}:{cascade}");
            Log.A("kill the very local temp cache");
            _list = new ImmutableArray<IEntity>();
            _listLoaded = false;
            Log.A("kill in list-cache");
            new ListCache(Log).Remove(this);
            if (cascade)
            {
                Log.A("tell upstream source to flush as well");
                Source.PurgeList(true);
            }
            callLog.Done("ok");
        }

        /// <inheritdoc />
        /// <summary>
        /// The source which holds this stream
        /// </summary>
		public IDataSource Source { get; }

        /// <inheritdoc />
        /// <summary>
        /// Name - usually the name within the Out-dictionary of the source. For identification and for use in caching-IDs and similar
        /// </summary>
		public string Name { get; }


        #region Support for IEnumerable<IEntity>

        public IEnumerator<IEntity> GetEnumerator() => List.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();
        #endregion Support for IEnumerable<IEntity>

        public ILog Log => Source?.Log; // Note that it can be null, but most commands on Log are null-safe
    }
}
