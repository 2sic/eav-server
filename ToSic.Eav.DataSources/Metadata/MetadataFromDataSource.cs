using ToSic.Eav.Metadata;

namespace ToSic.Eav.DataSources.Metadata
{
    /// <inheritdoc />
    /// <summary>
    /// This is a simple helper to ensure that objects can get metadata
    /// even if they do not know about data-sources
    /// this will then be provided through dependency injection
    /// </summary>
    public class MetadataFromDataSource: IRemoteMetadata
    {
        public IMetadataSource OfZoneAndApp(int zoneId, int appId) => DataSource.GetMetaDataSource(zoneId, appId);

        public IMetadataSource OfApp(int appId) => DataSource.GetMetaDataSource(null, appId);
    }
}
