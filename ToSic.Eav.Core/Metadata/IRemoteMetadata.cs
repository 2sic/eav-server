
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Metadata
{
    /// <summary>
    /// This interface allows objects to provide metadata from "remote" systems
    /// meaning from apps / sources which the original source doesn't know about
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public interface IRemoteMetadata
    {
        /// <summary>
        /// Get a metadata provider which uses data from a known zone and app
        /// </summary>
        /// <param name="zoneId">the zone id</param>
        /// <param name="appId">the app id</param>
        /// <returns>A <see cref="IMetadataSource"/> of that zone/app</returns>
        IMetadataSource OfZoneAndApp(int zoneId, int appId);

        /// <summary>
        /// Get a metadata provider which uses data from a known app, where the zone is not known
        /// </summary>
        /// <param name="appId">the app id</param>
        /// <returns>A <see cref="IMetadataSource"/> of that app</returns>
        IMetadataSource OfApp(int appId);
    }
}
