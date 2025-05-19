namespace ToSic.Lib.Core.Tests.LoggingTests;


public class LogAddInConstructor : LogTestBase
{
    public static IEnumerable<object?[]> SimpleMessages => TestDataMessages.SimpleMessages(Depth0);

    [Theory]
    [MemberData(nameof(SimpleMessages))]
    public void MessageInConstructor(string testName, string expected, string message, string result, int depth)
    {
        var log = new Log("", message: message);
        Single(log.Entries);
        AssertEntry(testName, log.Entries[0], expected, result, depth);
    }

}