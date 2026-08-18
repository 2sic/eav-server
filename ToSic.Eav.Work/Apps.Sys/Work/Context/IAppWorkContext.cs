using ToSic.Eav.DataSource;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppWorkContext : IAppIdentity
{
    IAppReader AppReader { get; }

    public DbStorage DbStorage { get; }
    IDataSource Data { get; }
}
