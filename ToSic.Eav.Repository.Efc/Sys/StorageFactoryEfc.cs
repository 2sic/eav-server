using ToSic.Eav.Repositories.Sys;

namespace ToSic.Eav.Repository.Efc.Sys;
internal class StorageFactoryEfc(Generator<DbStorage.DbStorage> genDbDataController) : ServiceBase("Eav.StoFacEfc"), IStorageFactory
{
    public IStorage New(StorageOptions options)
    {
        var dbDataController = genDbDataController.New();
        dbDataController.Init(options.ZoneId, options.AppId, options.ParentAppId);
        return dbDataController;
    }
}
