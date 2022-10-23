using ToSic.Lib.Documentation;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// Marks thing which belongs to an App and a Zone and know their full identity. 
    /// </summary>
    /// <remarks>
    /// Technically many things could just identify the app they belong to, and let the system look up the zone.
    /// But this would be inefficient, so for optimization, many items identify themselves with both the app and zone Ids
    /// </remarks>
    [PublicApi_Stable_ForUseInYourCode]
    public interface IAppIdentity: IZoneIdentity, IAppIdentityLight
    {
    }
}
