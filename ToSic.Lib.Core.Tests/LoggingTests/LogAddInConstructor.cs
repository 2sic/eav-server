using ToSic.Lib.Logging;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Lib.Core.Tests.LoggingTests;

[TestClass]
public class LogAddInConstructor : LogTestBase
{
    public static IEnumerable<object?[]> SimpleMessages => TestDataMessages.SimpleMessages(Depth0);

    [TestMethod]
    [DynamicData(nameof(SimpleMessages))]
    public void MessageInConstructor(string testName, string expected, string message, string result, int depth)
    {
        var log = new Log("", message: message);
        AreEqual(1, log.Entries.Count, testName);
        AssertEntry(testName, log.Entries[0], expected, result, depth);
    }

}