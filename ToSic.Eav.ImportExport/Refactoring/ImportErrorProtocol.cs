﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ToSic.Eav.ImportExport.Refactoring
{
    public class ImportErrorProtocol : IEnumerable<ImportError>
    {
        public ImportError this[int index]
        {
            get { return errors[index]; }
        }

        public List<ImportError> Errors
        {
            get { return errors; }
        }
        private readonly List<ImportError> errors = new List<ImportError>();

        public int ErrorCount
        {
            get { return errors.Count; }
        }

        public bool HasErrors
        {
            get { return Errors.Any(); }
        }

        public void AppendError(ImportErrorCode errorCode, string errorDetail = null, int? lineNumber = null, string lineDetail = null)
        {
            errors.Add(new ImportError(errorCode, errorDetail, lineNumber, lineDetail));
        }

        public IEnumerator<ImportError> GetEnumerator()
        {
            return errors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}