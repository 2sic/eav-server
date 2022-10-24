using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Logging;


namespace ToSic.Eav.Persistence.Logging
{
    public class ImportErrorLog : HasLog, IEnumerable<ImportError>
    {
        public ImportErrorLog(ILog parentLog = null) : base("Imp.ErrLog", parentLog) { }

        public ImportError this[int index] => Errors[index];

        public List<ImportError> Errors { get; } = new List<ImportError>();

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
}