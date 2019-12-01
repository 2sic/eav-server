﻿using ToSic.Eav.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Metadata
{
    /// <inheritdoc />
    /// <summary>
    /// This is a simple helper to ensure that objects can get metadata
    /// even if they do not know about data-sources
    /// this will then be provided through dependency injection
    /// </summary>
    [PublicApi]
    public class RemoteMetadataFromDataSource: IRemoteMetadata
    {
        public IMetadataSource OfZoneAndApp(int zoneId, int appId) => DataSource.GetMetaDataSource(zoneId, appId);

        public IMetadataSource OfApp(int appId) => DataSource.GetMetaDataSource(null, appId);
    }
}
