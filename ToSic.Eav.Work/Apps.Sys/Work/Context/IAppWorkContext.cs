using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppWorkContext : IAppDataContext
{
    public DbStorage DbStorage { get; }
}
