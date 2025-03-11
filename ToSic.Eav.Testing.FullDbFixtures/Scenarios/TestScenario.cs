namespace ToSic.Eav.Testing.Scenarios;

/// <summary>
/// The main configuration to use.
///
/// If you have special needs, create an instance and override.
///
/// @STV TODO: We should find a way (environment variable?) to return a different value on your PC for the DB etc.
/// </summary>
/// <remarks>
/// Do not call it `TestConfiguration` as it causes a lot of problems with `TestConfiguration` in the Microsoft-Testing namespace.
/// </remarks>
public record TestScenario
{
    /// <summary>
    /// Connection String to the DB
    /// </summary>
    public /*required*/ string ConStr { get; init; } = "";

    /// <summary>
    /// Global Data Folder with all the EAV/2sxc Content-Types and Settings
    /// </summary>
    public /*required*/ string GlobalFolder { get; init; } = "";

    /// <summary>
    /// Custom Data Folder for specific features/licenses etc.
    /// </summary>
    public /*required*/ string GlobalDataCustomFolder { get; init; } = "";

}