using ToSic.Eav.DataSource;

namespace ToSic.Eav.Apps.AppSys
{

    public interface IAppWorkCtx : IAppIdentity
    {
        AppState AppState { get; }

        bool? ShowDrafts { get; }

        IDataSource Data { get; }

    }
}