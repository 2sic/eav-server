using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.Persistence.Logging
{
    public class ImportErrorLog : IEnumerable<ImportError>
    {
        public ImportError this[int index] => _errors[index];

        public List<ImportError> Errors => _errors;
        private readonly List<ImportError> _errors = new List<ImportError>();

        public int ErrorCount => _errors.Count;

        public bool HasErrors => Errors.Any();

        public void AppendError(ImportErrorCode errorCode, string errorDetail = null, int? lineNumber = null, string lineDetail = null)
        {
            _errors.Add(new ImportError(errorCode, errorDetail, lineNumber, lineDetail));
        }

        public IEnumerator<ImportError> GetEnumerator()
        {
            return _errors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}