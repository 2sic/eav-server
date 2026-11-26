namespace ToSic.Eav.Apps.Sys.Caching;

[ShowApiWhenReleased(ShowApiMode.Never)]
internal class ZoneCachePurge(AppsCacheSwitch appsCacheSelector) : ServiceBase("App.SysMng"),
    IZoneCachePurge
{
    /// <inheritdoc/>
    public void PurgeZoneList()
        => appsCacheSelector.Value.PurgeZones();
}
