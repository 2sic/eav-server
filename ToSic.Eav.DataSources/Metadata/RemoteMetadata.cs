using ToSic.Eav.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Metadata
{
    /// <inheritdoc />
    /// <summary>
    /// Implementation of <see cref="IRemoteMetadata"/> to provide metadata globally. <br/>
    /// This ensures that EAV core objects can get metadata,
    /// even if they do not know about data-sources
    /// which will then be provided through dependency injection.
    /// </summary>
    [PublicApi]
    public class RemoteMetadata: IRemoteMetadata
    {
        /// <inheritdoc />
       public IMetadataSource OfZoneAndApp(int zoneId, int appId) 
            => DataSource.GetMetaDataSource(zoneId, appId);

        /// <inheritdoc />
        public IMetadataSource OfApp(int appId) 
            => DataSource.GetMetaDataSource(null, appId);
    }
}
