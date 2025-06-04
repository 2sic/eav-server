using ToSic.Eav.Repository.Efc;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Internal.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppWorkCtxWithDb : IAppWorkCtx
{
    DbStorage DbStorage { get; }
}