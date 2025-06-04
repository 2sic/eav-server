using ToSic.Eav.Persistence.Interfaces;
#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Repositories.Sys;
public class StorageFactoryUnknown(WarnUseOfUnknown<StorageFactoryUnknown> _) : ServiceBase("Eav.StoFac"), IStorageFactory
{
    public IStorage New(StorageOptions options)
    {
        return null;
    }
}
