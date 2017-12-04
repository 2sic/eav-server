namespace ToSic.Eav.Apps.Interfaces
{
    public interface IEnvironment<T>
    {
        IPermissions Permissions { get; }

        IZoneMapper<T> ZoneMapper { get; }

        IUser User { get; }

        IPagePublishing PagePublishing { get; }

        string MapPath(string virtualPath);
    }
}
