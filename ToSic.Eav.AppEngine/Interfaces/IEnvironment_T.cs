namespace ToSic.Eav.Apps.Interfaces
{
    public interface IEnvironment<T>: IEnvironment
    {
        new IZoneMapper<T> ZoneMapper { get; }

    }
}
