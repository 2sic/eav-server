namespace ToSic.Eav.Apps.Sys.Loaders;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IZoneLoader
{
    IDictionary<int, Zone> Zones();

}
