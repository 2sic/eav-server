namespace ToSic.Sys.Logging;

/// <summary>
/// Log settings are used to run certain tasks such as loading or saving data, and configure the level of detail which should be used.
/// </summary>
/// <param name="Enabled"></param>
/// <param name="Summary"></param>
/// <param name="Details"></param>
public record LogSettings(
    bool Enabled = true,
    bool Summary = true,
    bool Details = true
);
