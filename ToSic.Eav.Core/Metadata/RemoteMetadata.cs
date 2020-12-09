using ToSic.Eav.Apps;
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
    [PrivateApi] // 2020-12-09 v11.11 changed from [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public class RemoteMetadata: IRemoteMetadata
    {
        /// <inheritdoc />
        public IMetadataSource OfZoneAndApp(int zoneId, int appId) => State.Get(new AppIdentity(zoneId, appId));

        /// <inheritdoc />
        public IMetadataSource OfApp(int appId) => State.Get(appId);
    }
}
