namespace ToSic.Eav.Enums
{
    /// <summary>
    /// repositories which can contain content-types or entities
    /// </summary>
    public enum Repositories
    {
        Code, // data / CT is in code
        SystemFiles, // data from system files
        TestFiles, // test file system
        Sql // default

    }
}