using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Security;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Metadata
{
    /// <summary>
    /// Metadata of an item (a content-type or another entity). <br/>
    /// It's usually on a <strong>Metadata</strong> property of things that can have metadata.
    /// </summary>
    /// <typeparam name="T">The type this metadata uses as a key - int, string, guid</typeparam>
    /// <remarks>
    /// * Since v15.04 fully #immutable
    /// </remarks>
    [PrivateApi] // changed 2020-12-09 v11.11 from [PublicApi_Stable_ForUseInYourCode] - as this is a kind of lazy-metadata, we should change it to that
    public class MetadataOf<T> : IMetadataOf, IMetadataInternals, ITimestamped
    {

        #region Constructors

        /// <summary>
        /// Constructor that can take both a direct App-Source as well as a deferred source.
        /// Note that both sources can be null!
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="key"></param>
        /// <param name="title">Title of the target we're describing - for further automating when using or creating more Metadata</param>
        /// <param name="items">A direct list of items to use as metadata - instead of lazy-loading from a source. If specified, auto-sync will be disabled.</param>
        /// <param name="appSource"></param>
        /// <param name="deferredSource"></param>
        public MetadataOf(int targetType, T key, string title, List<IEntity> items = default, IHasMetadataSource appSource = default, Func<IHasMetadataSource> deferredSource = default)
        {
            _targetType = targetType;
            Key = key;
            _metadataTitle = title;

            // This is the implementation with a constant list, where no more lookups ever happen.
            _constantList = items;
            _appMetadataSource = appSource;
            _metaSourceRemote = deferredSource;
        }

        #endregion

        #region Debug Code to re-activate if every something is hard to decipher

        //public string Debug()
        //{
        //    return 
        //        $"{nameof(_debugAllEntry)}: {_debugAllEntry}, " +
        //        $"{nameof(_loadAllInLock.PreLockCount)}: {_loadAllInLock.PreLockCount}, " +
        //        $"{nameof(_loadAllInLock.LockCount)}: {_loadAllInLock.LockCount}" +
        //        $"{nameof(_debugAllReturn)}: {_debugAllReturn}, " +
        //        $"{nameof(_debugLoadFromProvider)}: {_debugLoadFromProvider}, " +
        //        $"{nameof(_debugUse)}: {_debugUse}, " +
        //        $"{nameof(CacheTimestamp)}: {CacheTimestamp} , " +
        //        $"{nameof(_appMetadataSource)}: {_appMetadataSource != null}, " +
        //        $"{nameof(_metaSourceRemote)}: {_metaSourceRemote != null}, " +
        //        $"{nameof(_allEntities)}: {_allEntities != null}, " +
        //        $"{nameof(_metadataWithoutPermissions)}: {_metadataWithoutPermissions != null}, " +
        //        $"{nameof(_mdsGetOnce)}: {_mdsGetOnce.IsValueCreated}, ";
        //}

        //private int _debugAllEntry;
        //private int _debugLoadFromProvider;
        //private int _debugAllReturn;
        //private int _debugUse;

        #endregion

        /// <summary>
        /// This is a constant / stable list of entities.
        /// When it is defined, no items are retrieved from the sources, since this is all that's ever used. 
        /// </summary>
        private readonly List<IEntity> _constantList;

        /// <summary>
        /// Type-information of the thing we're describing. This is used to retrieve metadata from the correct sub-list of pre-indexed metadata
        /// </summary>
        private readonly int _targetType;

        /// <summary>
        /// The key which identifies the item we're enriching with metadata
        /// </summary>
        protected T Key { get; }

        /// <summary>
        /// All entities is internal - because it contains everything
        /// including permissions-metadata
        /// </summary>
        [PrivateApi]
        public List<IEntity> AllWithHidden {
            get
            {
                if (_constantList != null) return _constantList;
                //_debugAllEntry++;
                // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
                _loadAllInLock.Go(() => _allCached == null || RequiresReload(), LoadFromProviderInsideLock);
                //_debugAllReturn++;
                return _allCached;
            }
        }
        private List<IEntity> _allCached;
        private readonly TryLockTryDo _loadAllInLock = new TryLockTryDo();

        /// <summary>
        /// All "normal" metadata entities - so it hides the system-entities
        /// like permissions. This is the default view of metadata given by an item
        /// </summary>
        private List<IEntity> MetadataWithoutPermissions
        {
            get
            {
                // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
                if (_metadataWithoutPermissions == null || RequiresReload())
                    _metadataWithoutPermissions = AllWithHidden
                        .Where(md => !Permission.IsPermission(md))
                        .ToList();
                return _metadataWithoutPermissions;
            }
        }
        private List<IEntity> _metadataWithoutPermissions;

        /// <inheritdoc />
        public IEnumerable<Permission> Permissions
        {
            get
            {
                if (_permissions == null || RequiresReload())
                    _permissions = AllWithHidden
                        .Where(Permission.IsPermission)
                        .Select(e => new Permission(e));
                return _permissions;
            }
        }
        private IEnumerable<Permission> _permissions;

        public long CacheTimestamp { get; private set; }

        [PrivateApi]
        protected bool RequiresReload() => _constantList == null && GetMetadataSource()?.CacheChanged(CacheTimestamp) == true;

        /// <summary>
        /// Load the metadata from the provider
        /// Must be virtual, because the inheriting <see cref="ContentTypeMetadata"/> needs to overwrite this. 
        /// </summary>
        [PrivateApi]
        protected virtual void LoadFromProviderInsideLock()
        {
            //_debugLoadFromProvider++;
            var mdProvider = GetMetadataSource();
            var list = mdProvider?.GetMetadata(_targetType, Key).ToList() ?? new List<IEntity>();
            Use(list);
            if (mdProvider != null) CacheTimestamp = mdProvider.CacheTimestamp;
        }

        /// <summary>
        /// Find the source of metadata. It may already be set, or it may not be available at all.
        /// </summary>
        /// <returns></returns>
        [PrivateApi]
        protected IMetadataSource GetMetadataSource() => _mdsGetOnce.Get(() => (_appMetadataSource ?? _metaSourceRemote?.Invoke())?.MetadataSource);
        private readonly GetOnce<IMetadataSource> _mdsGetOnce = new GetOnce<IMetadataSource>();
        /// <summary>
        /// The source (usually an app) which can provide all the metadata once needed
        /// </summary>
        private readonly IHasMetadataSource _appMetadataSource;
        private readonly Func<IHasMetadataSource> _metaSourceRemote;


        /// <summary>
        /// Set the local cache to a list of items to use as Metadata.
        /// </summary>
        /// <param name="items"></param>
        [PrivateApi]
        protected void Use(List<IEntity> items)
        {
            //_debugUse++;
            // Set the local cache to a list of items, and reset the dependent objects so they will be rebuilt if accessed.
            _allCached = items;
            _metadataWithoutPermissions = null;
            _permissions = null;
        }

        #region GetBestValue

        /// <inheritdoc />
        public TVal GetBestValue<TVal>(string name, string typeName = null)
        {
            var list = typeName == null ? MetadataWithoutPermissions : OfType(typeName);
            var found = list.FirstOrDefault(md => md.Attributes.ContainsKey(name));
            return found == null ? default : found.GetBestValue<TVal>(name, null);
        }

        /// <inheritdoc />
        public TVal GetBestValue<TVal>(string name, string[] typeNames)
        {
            foreach (var type in typeNames)
            {
                var result = GetBestValue<TVal>(name, type);
                if (!EqualityComparer<TVal>.Default.Equals(result, default))
                    return result;
            }
            return default;
        }
        #endregion

        #region Type Specific Data

        public bool HasType(string typeName) => this.Any(e => e.Type.Is(typeName));

        public IEnumerable<IEntity> OfType(string typeName) => MetadataWithoutPermissions.OfType(typeName);

        #endregion

        #region Target

        public ITarget Target => _target.Get(() => new Target(_targetType, _metadataTitle, Key));
        private readonly GetOnce<ITarget> _target = new GetOnce<ITarget>();
        private readonly string _metadataTitle;

        #endregion

        #region Context for data to be created

        public IAppIdentity Context(string type) => GetMetadataSource();
        public (int TargetType, List<IEntity> list, IHasMetadataSource appSource, Func<IHasMetadataSource> deferredSource) GetCloneSpecs() 
            => (_targetType, _constantList, _appMetadataSource, _metaSourceRemote);

        #endregion


        #region enumerators
        [PrivateApi]
        public IEnumerator<IEntity> GetEnumerator() => new EntityEnumerator(MetadataWithoutPermissions);

        [PrivateApi]
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
