namespace ToSic.Eav.Repositories.Sys;

public interface IStorageFactory
{
    IStorage New(StorageOptions options);
}