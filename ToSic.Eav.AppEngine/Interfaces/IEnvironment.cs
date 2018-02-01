namespace ToSic.Eav.Apps.Interfaces
{
    public interface IEnvironment
    {
        IPermissions Permissions { get; set; }

        IZoneMapper ZoneMapper { get; }

        IUser User { get; }

        IPagePublishing PagePublishing { get; }

        string MapPath(string virtualPath);

    }
}
