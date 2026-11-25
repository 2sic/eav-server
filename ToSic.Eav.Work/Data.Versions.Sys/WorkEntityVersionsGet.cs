using ToSic.Eav.DataSource.DataProviders;
using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Data.Versions.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkEntityVersionsGet(Generator<DbStorage, StorageOptions> dbGen)
    : ServiceWithSetup<IAppDataContext>("AWk.EntCre", connect: [dbGen]), IRawDataProviderWithCtx
{
    public const string NameId = "EntitiyVersions";

    IEnumerable<IRawData> IRawDataProviderWithCtx.Provide(IDataSourceConfiguration configuration)
        => VersionHistory(configuration.Get<int>(nameof(ItemHistory.EntityId)));

    internal List<Persistence.Versions.ItemHistory> VersionHistory(int id, bool includeData = true)
        => dbGen.New(new(MyOptions.AppReader)).Versioning.GetHistoryList(id, includeData);

}
