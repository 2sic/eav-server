using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppWorkCtxWithDb : IAppWorkCtx
{
    DbStorage DbStorage { get; }
}