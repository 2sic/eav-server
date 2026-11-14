namespace ToSic.Eav.Apps.Sys.Work;

/// <summary>
/// Simple low-threshold context providing access to the current App which should be used
/// for data retrieval only.
/// </summary>
/// <remarks>
/// This is to be used in work which only reads data and does not need access to any database or similar.
/// </remarks>
[PrivateApi("WIP 2026-11-14 2dm")]
public interface IAppDataContext : IAppIdentity
{
    /// <summary>
    /// The current AppReader, valid for the current context.
    /// </summary>
    IAppReader AppReader { get; }

    /// <summary>
    /// A data source for the current context, respecting the settings of draft-permissions.
    /// </summary>
    IDataSource Data { get; }
}
