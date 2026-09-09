namespace ToSic.Sys.Logging;

internal interface ILogInternal
{
    List<Entry> Entries { get; }

    Entry CreateAndAdd(string? message, CodeRef? code, EntryOptions? options, Entry? parent,
        Microsoft.Extensions.Logging.LogLevel level, Exception? exception, bool publish);

}
