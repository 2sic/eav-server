using System.Runtime.CompilerServices;

namespace ToSic.Lib.Core.Tests.LoggingTests;

public class LogTestBase
{
    public const int Depth0 = 0;
    public const string? ResultNone = null;

    protected string ThisMethodName([CallerMemberName] string? cName = null) => cName!;

    protected static void AssertLogIsEmpty(Log log, string testName) =>
        Empty(log.Entries);

    protected static void AssertEntry(string testName, Entry firstMsg, string? message, string? result, int depth)
    {
        Equal(message, firstMsg.Message);
        Equal(result, firstMsg.Result);
        Equal(depth, firstMsg.Depth);
    }

    internal void AssertDepthAndEntryCount(string testName, Log log, int logDepth, int entryCount)
    {
        Equal(logDepth, log.Depth);
        Equal(entryCount, log.Entries.Count);
    }

}