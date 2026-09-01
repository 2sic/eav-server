using ToSic.Eav.DataSource;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppWorkContext : IAppIdentity
{
    IAppReader AppReader { get; }

    public DbStorage DbStorage { get; }
    IDataSource Data { get; }

    /// <summary>
    /// New v22.0x: Creates a new DbStorage instance, which is a database storage context for the application.
    /// Reason is that we discovered that often a re-use can cause issues, so we try to better isolate cases where it's not needed to re-use the DbStorage instance.
    /// </summary>
    /// <returns></returns>
    DbStorage NewDbStorage();

    IAppWorkContext FreshContext(IAppReader? freshReader = null);
}
