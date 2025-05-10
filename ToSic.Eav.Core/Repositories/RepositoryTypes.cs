namespace ToSic.Eav.Repositories;

/// <summary>
/// repositories which can contain content-types or entities
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public enum RepositoryTypes
{
    Code, // data / CT is in code
    Folder, // data from system files
    Sql, // default
    TestingDoNotUse, // test file system
}