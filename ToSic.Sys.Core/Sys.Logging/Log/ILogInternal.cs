namespace ToSic.Sys.Logging;

internal interface ILogInternal
{
    List<Entry> Entries { get; }

    Entry CreateAndAdd(string? message, CodeRef? code, EntryOptions? options = default);

}