using ToSic.Eav.Interfaces;

namespace ToSic.Eav.DataSources.Metadata
{
    /// <inheritdoc />
    /// <summary>
    /// This is a simple helper to ensure that objects can get metadata
    /// even if they do not know about data-sources
    /// this will then be provided through dependency injection
    /// </summary>
    public class MetadataFromDataSource: IRemoteMetadataProvider
    {
        public IMetadataProvider OfZoneAndApp(int zoneId, int appId) => DataSource.GetMetaDataSource(zoneId, appId);

        public IMetadataProvider OfApp(int appId) => DataSource.GetMetaDataSource(null, appId);
    }
}
