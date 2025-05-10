using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps.Internal.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppWorkCtxWithDb : IAppWorkCtx
{
    DbDataController DataController { get; }
}