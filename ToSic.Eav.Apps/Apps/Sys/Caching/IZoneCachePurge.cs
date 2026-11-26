namespace ToSic.Eav.Apps.Sys.Caching;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IZoneCachePurge
{
    /// <summary>
    /// Flush the entire map of zones / apps
    /// </summary>
    void PurgeZoneList();
}
