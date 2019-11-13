using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// This thing belongs to an App and a Zone
    /// </summary>
    [PublicApi]
    public interface IInAppAndZone: IInZone, IInApp
    {
    }
}
