namespace ToSic.Eav.Persistence.Logging;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public record ImportError(
    ImportErrorCode ErrorCode,
    string? ErrorDetail = default,
    int? LineNumber = default,
    string? LineDetail = default);