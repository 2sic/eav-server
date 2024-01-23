using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Persistence.Logging;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ImportErrorLog(ILog parentLog) : HelperBase(parentLog, "Imp.ErrLog"), IEnumerable<ImportError>
{
    public ImportError this[int index] => Errors[index];

    public List<ImportError> Errors { get; } = [];

    public int ErrorCount => Errors.Count;

    public bool HasErrors => Errors.Any();

    public void Add(ImportErrorCode errorCode, string errorDetail = null, int? lineNumber = null, string lineDetail = null)
    {
        Errors.Add(new(errorCode, errorDetail, lineNumber, lineDetail));
        Log.A($"Imp-Err {errorCode} on line {lineDetail} details {lineDetail} msg: {errorDetail}");
    }

    public IEnumerator<ImportError> GetEnumerator() => Errors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Errors.GetEnumerator();
}