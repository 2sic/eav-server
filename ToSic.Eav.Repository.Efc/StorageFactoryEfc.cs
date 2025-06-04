using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Repository.Efc;

namespace ToSic.Eav;
internal class StorageFactoryEfc(Generator<DbDataController> genDbDataController) : ServiceBase("Eav.StoFacEfc"), IStorageFactory
{
    public IStorage New(StorageOptions options)
    {
        var dbDataController = genDbDataController.New();
        dbDataController.Init(options.ZoneId, options.AppId, options.ParentAppId);
        return dbDataController;
    }
}
