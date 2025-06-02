using ToSic.Eav.Persistence.Interfaces;

namespace ToSic.Eav.Repositories.Sys;

public interface IStorageFactory
{
    IStorage New(StorageOptions options);
}