using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Apps.Interfaces
{
    public interface IEnvironment
    {
        IZoneMapper ZoneMapper { get; }

        IUser User { get; }

        IPagePublishing PagePublishing { get; }

        string MapPath(string virtualPath);

    }
}
