namespace ToSic.Eav.Apps.Interfaces
{
    public interface IEnvironment
    {
        IPermissions Permissions { get; }

        IZoneMapper ZoneMapper { get; }

        IUser User { get; }
    }
}
