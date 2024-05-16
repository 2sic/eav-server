using ToSic.Eav.Code.Infos;

namespace ToSic.Eav.Code.InfoSystem;

/// <summary>
/// Package to hold the log-entry in the history/store together with the code use itself.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class CodeInfoInLogStore(CodeUse use, LogStoreEntry entry = default)
{
    public CodeUse Use { get; } = use;
    public LogStoreEntry EntryOrNull { get; } = entry;
}