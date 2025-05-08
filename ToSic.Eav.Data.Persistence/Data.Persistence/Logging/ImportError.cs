namespace ToSic.Eav.Persistence.Logging;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ImportError(
    ImportErrorCode errorCode,
    string errorDetail = null,
    int? lineNumber = null,
    string lineDetail = null)
{
    public int? LineNumber { get; private set; } = lineNumber;

    public string LineDetail { get; private set; } = lineDetail;

    public string ErrorDetail { get; private set; } = errorDetail;

    public ImportErrorCode ErrorCode { get; private set; } = errorCode;
}