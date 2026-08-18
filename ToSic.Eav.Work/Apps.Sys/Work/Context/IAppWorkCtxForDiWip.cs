using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppWorkCtxForDiWip : IAppIdentity
{
    IAppReader AppReader { get; }

    public DbStorage DbStorage { get; }
}
