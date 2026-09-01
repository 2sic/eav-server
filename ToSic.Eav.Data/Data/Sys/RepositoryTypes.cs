namespace ToSic.Eav.Data.Sys;

/// <summary>
/// Repositories which are the source of data.
/// </summary>
/// <remarks>
/// This is used to specify where data comes from.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public enum RepositoryTypes
{
    /// <summary>
    /// Unknown Repository Type - should never be used.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Data or Content-Types are generated in code using configuration (usually using c# attributes).
    /// </summary>
    CodeConfiguration,

    /// <summary>
    /// Data or Content-Types are generated on the fly using reflection.
    /// </summary>
    CodeReflection,

    /// <summary>
    /// Data or Content-Types are generated in code, where the code explicitly defines the data or content-types.
    /// </summary>
    CodeExplicit,

    /// <summary>
    /// Data or Content-Types are from a folder (file system)
    /// </summary>
    Folder,

    /// <summary>
    /// Data or Content-Types are from SQL (default)
    /// </summary>
    Sql,

    /// <summary>
    /// Virtual source for certain tests.
    /// </summary>
    TestingDoNotUse,
}