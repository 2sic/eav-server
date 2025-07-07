namespace ToSic.Sys.Logging;

public record LogSettings(
    bool Enabled = true,
    bool Summary = true,
    bool Details = true
);
