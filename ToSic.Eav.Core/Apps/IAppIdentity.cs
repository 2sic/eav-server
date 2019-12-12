using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// Marks thing which belongs to an App and a Zone
    /// </summary>
    /// <remarks>
    /// Technically many things could just identify the app they belong to, and let the system look up the zone.
    /// But this would be inefficient, so for optimization, many items identify themselves with both the app and zone Ids
    /// </remarks>
    [PublicApi]
    public interface IAppIdentity: IZoneIdentity, IAppId
    {
    }
}
