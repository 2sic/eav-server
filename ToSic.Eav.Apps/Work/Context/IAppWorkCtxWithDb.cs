using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps.Work
{
    public interface IAppWorkCtxWithDb : IAppWorkCtx
    {
        DbDataController DataController { get; }
    }
}
