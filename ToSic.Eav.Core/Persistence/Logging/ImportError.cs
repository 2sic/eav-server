namespace ToSic.Eav.Persistence.Logging;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ImportError
{
    public int? LineNumber { get; private set; }

    public string LineDetail { get; private set; }

    public string ErrorDetail { get; private set; }

    public ImportErrorCode ErrorCode { get; private set; }

    public ImportError(ImportErrorCode errorCode, string errorDetail = null, int? lineNumber = null, string lineDetail = null)
    {
        ErrorCode = errorCode;
        ErrorDetail = errorDetail;
        LineNumber = lineNumber;
        LineDetail = lineDetail;
    }
}