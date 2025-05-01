using ToSic.Eav.Data;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.Metadata;

partial class MetadataOf<T>
{
    /// <summary>
    /// All entities is internal - because it contains everything
    /// including permissions-metadata
    /// </summary>
    [PrivateApi]
    public List<IEntity> AllWithHidden
    {
        get
        {
            //_debugAllEntry++;
            // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
            field = _loadAllInLock.Call(
                    conditionToGenerate: () => field == null || UpStreamChanged(),
                    generator: LoadAndResetInLock,
                    cacheOrFallback: () => field ?? []
                )
                .Result;

            //_debugAllReturn++;
            return field;
        }
    }

    private readonly TryLockTryDo _loadAllInLock = new();
        
    public long CacheTimestamp { get; private set; }

    [PrivateApi]
    private bool UpStreamChanged() => Source.CacheChanged(CacheTimestamp);

    private List<IEntity> LoadAndResetInLock()
    {
        var result = LoadFromProviderInsideLock() ?? [];
            
        // Reset everything and possibly also the timestamp
        _metadataWithoutPermissions = null;
        _permissions = null;
        CacheTimestamp = GetMetadataSource()?.CacheTimestamp ?? CacheTimestamp;

        return result;
    }

    /// <summary>
    /// Load the metadata from the provider
    /// Must be virtual, because the inheriting <see cref="ContentTypeMetadata"/> needs to overwrite this. 
    /// </summary>
    /// <returns>The cached final list, for overloads which need to verify that something arrived.</returns>
    [PrivateApi]
    protected virtual List<IEntity> LoadFromProviderInsideLock(IList<IEntity> additions = default)
    {
        //_debugLoadFromProvider++;
        var mdProvider = GetMetadataSource();
        var mdOfKey = Source.SourceDirect?.List
                      ?? mdProvider?.GetMetadata(_targetType, Key)
                      ?? [];
        //_debugUse++;
        return (additions == null ? mdOfKey : mdOfKey.Concat(additions)).ToList();
    }

    /// <summary>
    /// Find the source of metadata. It may already be set, or it may not be available at all.
    /// </summary>
    /// <returns></returns>
    [PrivateApi]
    protected IMetadataSource GetMetadataSource() => _mdsGetOnce.Get(() => Source.MainSource?.MetadataSource);
    private readonly GetOnce<IMetadataSource> _mdsGetOnce = new();

}