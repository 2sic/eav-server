using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav.Apps.Work;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppWorkCtxWithDb : IAppWorkCtx
{
    DbDataController DataController { get; }
}