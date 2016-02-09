using System;
using System.Diagnostics;

namespace ToSic.Eav.Import
{
    public class ImportLogItem
    {
        public EventLogEntryType EntryType { get; private set; }
        public ImportEntity ImportEntity { get; set; }
        public ImportAttributeSet ImportAttributeSet { get; set; }
        public ImportAttribute ImportAttribute { get; set; }
        public IValueImportModel Value { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; private set; }

        public ImportLogItem(EventLogEntryType entryType, string message, ImportEntity importEntity = null)
        {
            EntryType = entryType;
            Message = message;
            ImportEntity = importEntity;
        }
    }
}
