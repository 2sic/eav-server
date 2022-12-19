using System.Collections.Generic;

namespace ToSic.Lib.Logging
{
    internal interface ILogInternal
    {
        List<Entry> Entries { get; }

        ILog Parent { get; }

    }
}
