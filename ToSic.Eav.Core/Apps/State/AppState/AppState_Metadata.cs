using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps.State;

partial class AppState:
    IHasMetadata,
    IHasMetadataSourceAndExpiring
{
    ///// <inheritdoc />
    //public IEnumerable<IEntity> GetMetadata<TMetadataKey>(int targetType, TMetadataKey key, string contentTypeName = null) 
    //    => _metadataManager.GetMetadata(targetType, key, contentTypeName);

    ///// <inheritdoc />
    //public IEnumerable<IEntity> GetMetadata<TKey>(TargetTypes targetType, TKey key, string contentTypeName = null)
    //    => _metadataManager.GetMetadata(targetType, key, contentTypeName);
        
    [PrivateApi]
    internal IMetadataOf GetMetadataOf<T>(TargetTypes targetType, T key, string title = null)
        => new MetadataOf<T>((int)targetType, key, title, appSource: _metadataManager);

    private AppMetadataManager _metadataManager;

    /// <summary>Metadata describing this App</summary>
    public IMetadataOf Metadata { get; private set; }

    /// <inheritdoc />
    public IMetadataSource MetadataSource => _metadataManager;

}