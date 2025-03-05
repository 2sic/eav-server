using ToSic.Lib.Logging;
using static Xunit.Assert;


namespace ToSic.Lib.Core.Tests.LoggingTests;

public class LogTestBase: LibTestBase
{
    public const int Depth0 = 0;
    public const string? ResultNone = null;

    protected static void AssertLogIsEmpty(Log log, string testName)
    {
        Equal(0, log.Entries.Count);
    }

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