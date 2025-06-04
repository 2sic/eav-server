namespace ToSic.Eav.Persistence.Sys.Logging;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record ImportError(
    ImportErrorCode ErrorCode,
    string? ErrorDetail = default,
    int? LineNumber = default,
    string? LineDetail = default);