using ToSic.Eav.Code.Infos;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Code.InfoSystem;

/// <summary>
/// Package to hold the log-entry in the history/store together with the code use itself.
/// </summary>
public class CodeInfoInLogStore
{
    public CodeInfoInLogStore(CodeUse use, LogStoreEntry entry = default)
    {
        Use = use;
        EntryOrNull = entry;
    }

    public CodeUse Use { get; }
    public LogStoreEntry EntryOrNull { get; }
}