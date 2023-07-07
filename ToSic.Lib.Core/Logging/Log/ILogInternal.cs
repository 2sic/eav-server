using System.Collections.Generic;

namespace ToSic.Lib.Logging
{
    internal interface ILogInternal
    {
        List<Entry> Entries { get; }

        Entry CreateAndAdd(string message, CodeRef code, EntryOptions options = default);

    }
}
