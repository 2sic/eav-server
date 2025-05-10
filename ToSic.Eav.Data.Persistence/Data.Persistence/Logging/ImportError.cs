namespace ToSic.Eav.Persistence.Logging;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record ImportError(
    ImportErrorCode ErrorCode,
    string? ErrorDetail = default,
    int? LineNumber = default,
    string? LineDetail = default);