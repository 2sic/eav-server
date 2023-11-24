using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Persistence.Logging;

public class ImportErrorLog : HelperBase, IEnumerable<ImportError>
{
    public ImportErrorLog(ILog parentLog) : base(parentLog, "Imp.ErrLog") { }

    public ImportError this[int index] => Errors[index];

    public List<ImportError> Errors { get; } = new();

    public int ErrorCount => Errors.Count;

    public bool HasErrors => Errors.Any();

    public void Add(ImportErrorCode errorCode, string errorDetail = null, int? lineNumber = null, string lineDetail = null)
    {
        Errors.Add(new ImportError(errorCode, errorDetail, lineNumber, lineDetail));
        Log.A($"Imp-Err {errorCode} on line {lineDetail} details {lineDetail} msg: {errorDetail}");
    }

    public IEnumerator<ImportError> GetEnumerator() => Errors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Errors.GetEnumerator();
}