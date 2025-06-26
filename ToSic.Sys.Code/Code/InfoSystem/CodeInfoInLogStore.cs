using ToSic.Sys.Code.Infos;

namespace ToSic.Sys.Code.InfoSystem;

/// <summary>
/// Package to hold the log-entry in the history/store together with the code use itself.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class CodeInfoInLogStore(CodeUse use, LogStoreEntry? entry = default)
{
    public CodeUse Use { get; } = use;
    public LogStoreEntry? EntryOrNull { get; } = entry;
}