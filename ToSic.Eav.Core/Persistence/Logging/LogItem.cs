﻿using System;
using System.Diagnostics;
using ToSic.Eav.Data;

namespace ToSic.Eav.Persistence.Logging
{
    public class LogItem
    {
        public EventLogEntryType EntryType { get; private set; }

        public ContentType ContentType { get; set; }
        public string ImpAttribute { get; set; }
        public IValue ImpValue { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; private set; }

        public LogItem(EventLogEntryType entryType, string message)
        {
            EntryType = entryType;
            Message = message;
        }
    }
}
