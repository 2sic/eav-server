using ToSic.Eav.Metadata;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.State;

partial class AppState: IMetadataSource, IHasMetadata, IMetadataOfSource, IHasMetadataSource
{
    /// <inheritdoc />
    public IEnumerable<IEntity> GetMetadata<TMetadataKey>(int targetType, TMetadataKey key, string contentTypeName = null) 
        => _metadataManager.GetMetadata(targetType, key, contentTypeName);

    /// <inheritdoc />
    public IEnumerable<IEntity> GetMetadata<TKey>(TargetTypes targetType, TKey key, string contentTypeName = null)
        => _metadataManager.GetMetadata(targetType, key, contentTypeName);
        
    [PrivateApi]
    public IMetadataOf GetMetadataOf<T>(TargetTypes targetType, T key, string title = null) => new MetadataOf<T>((int)targetType, key, title, appSource: this);

    private AppMetadataManager _metadataManager;

    /// <summary>Metadata describing this App</summary>
    public IMetadataOf Metadata { get; private set; }

    /// <inheritdoc />
    IMetadataSource IHasMetadataSource.MetadataSource => _metadataManager;

}