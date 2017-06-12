using System;
using System.Diagnostics;
using ToSic.Eav.ImportExport.Models;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.ImportExport.Logging
{
    public class ImportLogItem
    {
        public EventLogEntryType EntryType { get; private set; }
        //public ImpEntity ImpEntity { get; set; }
        public ImpContentType ImpContentType { get; set; }
        public string ImpAttribute { get; set; }
        public IValue ImpValue { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; private set; }

        public ImportLogItem(EventLogEntryType entryType, string message/*, ImpEntity impEntity = null*/)
        {
            EntryType = entryType;
            Message = message;
            //ImpEntity = impEntity;
        }
    }
}
