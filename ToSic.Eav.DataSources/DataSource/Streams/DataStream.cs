using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.DataSource.Caching;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSource.Streams
{
    /// <summary>
    /// A DataStream to get Entities when needed
    /// </summary>
    [PrivateApi]
    public class DataStream : IDataStream, IHasLog
    {
        private readonly Func<IImmutableList<IEntity>> _listDelegate;

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
        public string Scope { get; protected internal set; } = Scopes.Default;

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
        public DataStream(IDataSource source, string name, Func<IEnumerable<IEntity>> listDelegate = null, bool enableAutoCaching = false)
            : this(source, name, ConvertDelegate(listDelegate), enableAutoCaching) { }

        /// <summary>
        /// Constructs a new DataStream
        /// </summary>
        /// <param name="source">The DataSource providing Entities when needed</param>
        /// <param name="name">Name of this Stream</param>
        /// <param name="listDelegate">Function which gets Entities</param>
        /// <param name="enableAutoCaching"></param>
        public DataStream(IDataSource source, string name, Func<IImmutableList<IEntity>> listDelegate = null, bool enableAutoCaching = false)
        {
            Source = source;
            Name = name;
            _listDelegate = listDelegate;
            AutoCaching = enableAutoCaching;
        }

        private static Func<IImmutableList<IEntity>> ConvertDelegate(Func<IEnumerable<IEntity>> original)
        {
            if (original == null) return null;
            return () =>
            {
                var initialResult = original();
                return initialResult as IImmutableList<IEntity> ?? initialResult.ToImmutableList();
            };
        }

        #region Get Dictionary and Get List

        public IEnumerable<IEntity> List => _list.GetM(Log, parameters: $"{nameof(Name)}:{Name}", timer: true, generator: _ =>
        {
            // Check if it's in the cache - and if yes, if it's still valid and should be re-used --> return if found
            if (!AutoCaching) return (ReadUnderlyingList(), $"read; no {nameof(AutoCaching)}");

            var cacheItem = new ListCache(Log).GetOrBuild(this, ReadUnderlyingList, CacheDurationInSeconds);
            return (cacheItem.List, $"with {nameof(AutoCaching)}");
        });

        /// <summary>
        /// A temporary result list - must be a List, because otherwise
        /// there's a high risk of IEnumerable signatures with functions being stored inside.
        /// 
        /// Note that were possible, it will be an ImmutableSmartList wrapping an ImmutableArray for maximum performance.
        /// </summary>
        private readonly GetOnce<IImmutableList<IEntity>> _list = new GetOnce<IImmutableList<IEntity>>();

        /// <summary>
        /// Assemble the list - from the initially configured ListDelegate
        /// </summary>
        /// <returns></returns>
        private IImmutableList<IEntity> ReadUnderlyingList()
        {
            var l = Log.Fn<IImmutableList<IEntity>>();
            // try to use the built-in Entities-Delegate, but if not defined, use other delegate; just make sure we test both, to prevent infinite loops

            IImmutableList<IEntity> CreateErr(string title, string message, Exception ex = default)
                => Source.Error.Create(source: Source, title: title, message: message, exception: ex).ToImmutableList();

            if (_listDelegate == null)
                return l.ReturnAsError(CreateErr("Error loading Stream",
                    "Can't load stream - no delegate found to supply it"));

            try
            {
                var resultList = ImmutableSmartList.Wrap(_listDelegate());
                return l.ReturnAsOk(resultList);
            }
            catch (InvalidOperationException invEx) // this is a special exception - for example when using SQL. Pass it on to enable proper testing
            {
                return l.ReturnAsError(CreateErr("InvalidOperationException", "See details", invEx));
            }
            catch (Exception ex)
            {
                return l.ReturnAsError(CreateErr("Error getting Stream / reading underlying list",
                    $"Error getting List of Stream.\nStream Name: {Name}\nDataSource Name: {Source.Name}", ex));
            }
        }
        #endregion



        public void PurgeList(bool cascade = false) => Log.Do(message: $"PurgeList on Stream: {Name}, {nameof(cascade)}:{cascade}", action: l =>
        {
            l.A("kill the very local temp cache");
            _list.Reset();
            l.A("kill in list-cache");
            new ListCache(Log).Remove(this);
            if (!cascade) return;
            l.A("tell upstream source to flush as well");
            Source.PurgeList(true);
        });

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


        public IDataSourceLink Link => _link.Get(() => new DataSourceLink(null, dataSource: Source, stream: this, outName: Name));
        private readonly GetOnce<IDataSourceLink> _link = new GetOnce<IDataSourceLink>();
    }
}
